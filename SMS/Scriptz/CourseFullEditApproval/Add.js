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
        swal({
            title: "Are you sure?",
            text: "You will not be able to recover this imaginary file!",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55",
            confirmButtonText: "Yes, delete it!",
            closeOnConfirm: false
        },
        function () {
            swal("Deleted!", "Your imaginary file has been deleted.", "success");
        });
    }

})