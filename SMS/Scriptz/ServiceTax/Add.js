$(document).ready(function () {

    //toastr options
    toastr.options = {
        "closeButton": true,
        "positionClass": "toast-bottom-right",
        "progressBar": true
    }

    //datepicker plugin
    $("#txtSTDate").datepicker({
        startDate: new Date(),
        autoclose:true
    });

    //inputmask    
    $("#txtSTDate").inputmask("mm/dd/yyyy", { "placeholder": "mm/dd/yyyy" })

    //calling validation function of textbox change
    $("#txtSTDate").change(function (evt, params) {
        $(evt.target).valid();
    });

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
                    else if (data.message == "warning") {
                        toastr.info("Details already exists")
                    }
                    else if (data.message == "error") {
                        toastr.info("Error:Something gone wrong")
                    }
                    else {
                        toastr.error("Exception:Something gone wrong")
                    }
                },
                error: function (data) {
                    isLoading(false);
                    toastr.error("Exception:some thing gone wrong");
                }
            });
        }

        return false;
    }

    //clear all controls
    var clearAll = function () {       
        $(".form-control").val('');
        return false;
    };

    //insert event on submit  click 
    $(document).on("click", "#btnSubmit", ajaxInsert);

    //clear all on on cancel  click 
    $(document).on("click", "#btnCancel", clearAll);

});