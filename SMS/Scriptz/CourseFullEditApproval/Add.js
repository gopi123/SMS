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
                        ajaxUpdate();
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

                    
                });
            }
            
        });
    }

    

    var ajaxUpdate = function () 
    {
        
        
        alert("processing.....");
    }


})