// COURSE FULL EDIT APPROVAL => ADD.JS
$(function () {

    var form = $("#frmAdd");
    form.validate({
        errorPlacement: function errorPlacement(error, element) { element.before(error); }
    });
    form.find("#FormWithStepz").steps({
        headerTag: "h3",
        bodyTag: "section",
        transitionEffect: "slideLeft",
        onStepChanging: function (event, currentIndex, newIndex) {

            if (currentIndex > newIndex) {
                return true;
            }
            $(form).data('validator', null);
            $.validator.unobtrusive.parse($('form'));
            form.validate().settings.ignore = ":disabled,:hidden";

            if (currentIndex == 0 || currentIndex == 1) {
                return true;
            }

        },
        onFinishing: function (event, currentIndex) {
            $(form).data('validator', null);
            $.validator.unobtrusive.parse($('form'));
            form.validate().settings.ignore = ":disabled,:hidden";

            if (form.valid()) {

                return true;

            }
            else {
                return false;
            }

        },
        onFinished: function (event, currentIndex) {
            ShowApprovalAlert();

        }
    });

    //text-input to uppercase
    $(".form-control").addClass('capitalise');

    //iCheck for checkbox and radio inputs
    $('.minimal').iCheck({
        checkboxClass: 'icheckbox_square-blue',
        increaseArea: '20%'
    });

    $('.minimal').on('ifChecked', function (event) {
        $(event.target).valid();
    });

    //toastr options
    toastr.options = {
        "closeButton": true,
        "positionClass": "toast-bottom-right",
        "progressBar": true
    }


    var ShowApprovalAlert = function () {
        //initial approval or reject popup
        swal({
            title: "Approve or Reject?",
            text: "Please click below to approve or reject",
            type: "info",
            showCancelButton: true,
            confirmButtonColor: "#55dd7a",
            confirmButtonText: "Approve",
            cancelButtonText:"Reject",
            closeOnConfirm: false,
            closeOnCancel: false
        },
        function (isConfirm) {
            if (isConfirm) {
                //on clicking approve button
                swal({
                    title: "Are you sure?",
                    text: "You will not be able to undo the change!",
                    type: "warning",
                    showCancelButton: true,
                    confirmButtonColor: "#55dd7a",
                    confirmButtonText: "Yes, approve it!",
                    closeOnConfirm: true
                },
                function () {
                   
                    setTimeout(function () {
                        ajaxApproval();
                    }, 250);
                    
                });
            }
            else {
                //on clicking delete button
                swal({
                    title: "Rejected Reason!",
                    text: "Enter reason for rejection:",
                    type: "input",
                    showCancelButton: true,
                    confirmButtonColor: "#DD6B55",
                    confirmButtonText: "Reject it!",
                    closeOnConfirm: false,
                    animation: "slide-from-top",
                    inputPlaceholder: "Enter reason"
                },
                function (inputValue) {
                    if (inputValue === false) return false;

                    if (inputValue === "") {
                        swal.showInputError("Please enter reason!");
                        return false
                    }
                    else {
                        $("#hFieldRejectedReason").val(inputValue);
                    }
                    swal.close();
                    setTimeout(function () {
                        ajaxReject();
                    }, 250);
                });
            }
            
        });
    }

    var ajaxApproval = function () {
        console.log($("#hFieldRejectedReason").val());
        var form = $("#frmAdd");
        var redirectUrl = $(form).data("redirect-url");
        var href = $(form).data("approval-url");
        var formData = $(form).serialize();

        $.blockUI({ message: '<h3><img src="../plugins/jQueryBlockUI/images/busy.gif" /> <b>Please wait... </b></h3>' });

        $.ajax({

            type: "POST",
            url: href,
            data: formData,
            datatype: "json",

            success: function (data) {
               
                if (data.Status == "success") {
                    sendEmail(data.RegistrationId);

                }
                else {
                    $.unblockUI();
                    toastr.warning("Successfully saved the Student Registration.But error while sending email or sms");
                }
            },

            error: function (err) {
                toastr.error("Error:" + this.message)
            }
        });
    };
    
    //Code for sending reject 
    var ajaxReject = function () {

        console.log($("#hFieldRejectedReason").val());
        var form = $("#frmAdd");
        var redirectUrl = $(form).data("redirect-url");
        var href = $(form).data("reject-url");
        var formData = $(form).serialize();

        $.blockUI({ message: '<h3><img src="../plugins/jQueryBlockUI/images/busy.gif" /> <b>Please wait... </b></h3>' });

        $.ajax({

            type: "POST",
            url: href,
            data: formData,
            datatype: "json",
            
            success: function (data) {
               
                if (data.Status == "success") {
                    //send email code goes here
                    sendEmail(data.RegistrationId);

                }
                else {
                    $.unblockUI();
                    toastr.warning("Successfully saved the Student Registration.But error while sending email or sms");
                }
            },

            error: function (err) {
                toastr.error("Error:" + this.message)
            }
        });
    };

    var sendEmail = function (studRegId) {
        var href = $("#frmAdd").data("mail-send-url");       
        $.ajax({
            type: "GET",
            url: href,
            data: { regId: studRegId},
            datatype: "json",
            success: function (data) {
                $.unblockUI();
                if (data == "success") {
                    toastr.success("Successfully saved the details.");
                    setTimeout(function () {
                        var url = $(form).data("redirect-url");                       
                        window.location.href = url;
                    }, 2000);

                }
                else {
                    toastr.warning("Successfully saved the Student Registration.But error while sending email or sms");
                }
                //toastr.success("Successfully saved the details.")


            },
            error: function (err) {
                toastr.error("Error:" + this.message)
            }
        });
    }


})