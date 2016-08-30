
//GROUP => ADD.JS

$(function () {

    

    $("#ddlCentreCode").select2({
        placeholder: "Select Course Code",
        allowClear: true
    });

    $("#ddlCentreCode").change(function (evt, params) {
        if ($(this).val() != "" && $(this).val() != null) {
            $(evt.target).valid();
        }
    });

    $(".form-control").addClass('capitalise');

    //toastr options
    toastr.options = {
        "closeButton": true,
        "positionClass": "toast-bottom-right",
        "progressBar": true
    }

    //setting footer button disabled after save click
    var disableFooterButton = function () {       
        $(".footerbutton").addClass('disabled');
    };

    var clearAll = function () {
        $(".form-control").val("");
        $("#ddlCentreCode").select2("val", "");
    };

    var AddGroup = function () {

        var form = $("#frmAdd");
        var href = $(form).data('url');

        

        if (form.valid()) {
            //isLoading(true);            
            var formData = $(form).serialize();

            $.blockUI({ message: '<h3><img src="../plugins/jQueryBlockUI/images/busy.gif" /> <b>Please wait... </b></h3>' });
            $.ajax({
                type: "POST",
                url: href,
                data: formData,
                datatype: "json",
                success: function (data) {
                    $.unblockUI();

                    //isLoading(false);
                    if (data == "success") {
                        disableFooterButton();
                        toastr.success("Successfully saved the details.")
                        setTimeout(function () {
                            var url = $(form).data("get-url");                           
                            window.location.href = url;
                        }, 2000);
                    }
                    else {
                        toastr.error("Error:Something gone wrong")
                    }
                },
                error: function (data) {
                    $.unblockUI();
                    toastr.error("Exception:some thing gone wrong");
                }
            });


        };
    }  

    $("#btnSubmit").click(function () {
        AddGroup();
    });

    $("#btnCancel").click(function () {
        clearAll();
    });

});