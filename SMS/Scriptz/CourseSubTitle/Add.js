$(document).ready(function () {

    //text-input to uppercase
    $(".form-control").addClass('capitalise');

    //Select2 for dropdownlist
    $("#ddlCourseType").select2({
        placeholder: "Select a Course type",
        allowClear: true
    });

    $("#ddlCourseSeriesType").select2({
        placeholder: "Select a Course Series",
        allowClear: true
    });

    //show or hide validation on dropdown change
    $("#ddlCourseType").change(function (evt, params) {
        if ($(this).val() != "") {
            $(evt.target).valid();
        }        
    });
    $("#ddlCourseSeriesType").change(function (evt, params) {
        if ($(this).val() != "") {
            $(evt.target).valid();
        }
    });

    //toastr options
    toastr.options = {
        "closeButton": true,
        "positionClass": "toast-bottom-right",
        "progressBar": true
    }

    //IsLoading function
    function isLoading(loading) {
        if (loading) {
            $('.content').loading({
                message: "Please wait ...",
                theme: 'dark'
            });
        }
        else {
            $('.content').loading('stop');
        }
    }

    var clearAll = function () {
        //resetting dpdwndistrict for clearing the selected option
        $("#ddlCourseType").select2("val", "");
        $("#ddlCourseSeriesType").select2("val", "");
        $(".form-control").val('');
        return false;
    };

    //ajax insert
    var ajaxInsert = function () {        
        var form = $(this).closest('form');
        var href = $(form).data('url');
       
        if (form.valid()) {           
            isLoading(true);
            var formData = $(form).serialize();
            $.ajax({
                type: "POST",
                url: href,
                data: formData,
                datatype: "json",
                success: function (data) {
                    isLoading(false);
                    if (data.message == "success") {
                        clearAll();
                        toastr.success("Successfully saved the details.")
                    }
                    else {
                        toastr.error("Error:Something gone wrong")
                    }
                },
                error: function (data) {
                    isLoading(false);
                    toastr.error("Exception:Something gone wrong");
                }
            });
        }

        return false;
    }

    //clear all on on cancel  click 
    $(document).on("click", "#btnCancel", clearAll);

    //insert event on submit  click 
    $(document).on("click", "#btnSubmit", ajaxInsert);
});