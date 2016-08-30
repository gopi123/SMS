
//LEADER APPROVAL.JS

$(function () {

    //iCheck for checkbox and radio inputs
    $('input[type="radio"].minimal').iCheck({
        radioClass: 'iradio_square-blue',
        increaseArea: '20%'
    });

    //text-input to uppercase
    $(".toUpper").addClass('capitalise');

    //toastr options
    toastr.options = {
        "closeButton": true,
        "positionClass": "toast-bottom-right",
        "progressBar": true
    }

    $(document).on('ifChecked', '.minimal', function (event) {
        $(event.target).valid();

        var currSelected = $(this).val();
        var $dislikeDiv = $(this).closest('.parentRow').find('.dislikeReasonDiv');

        if (currSelected == "False") {
            $dislikeDiv.slideDown();
        }
        else {
            $dislikeDiv.slideUp();
        }
        return false;

    });

    $(document).on("click", "#btnSubmit", function () {
        var form = $("#frmAdd");
        var bootboxMsg = "";
        if (form.valid()) {

            if (isApproved()) {
                bootboxMsg = "Are you sure you want to <b>APPROVE</b> this project report";
            }
            else {
                bootboxMsg = "Are you sure you want to <b>DENY</b> this project report";
            }

            bootbox.confirm(bootboxMsg, function (result) {
                if (result) {
                    $.blockUI({ message: '<h3><img src="../plugins/jQueryBlockUI/images/busy.gif" /> <b>Please wait... </b></h3>' });
                    var href = $(form).data("url");
                    var formData = $(form).serialize();
                    var redirectUrl = $(form).data("redirect-url");
                    $.ajax({
                        type: "POST",
                        url: href,
                        data: formData,
                        datatype: "json",
                        success: function (data) {
                            $.unblockUI();
                            if (data.message == "success") {
                                toastr.success("Successfully saved the details.");
                                setTimeout(function () {
                                    window.location.href = redirectUrl;
                                }, 2000);

                            }
                            else {
                                toastr.error("Error:Something gone wrong")
                            }
                        },
                        error: function (data) {
                            $.unblockUI();
                            toastr.error("Exception:Something gone wrong");
                        }
                    });
                }
            });


        };


        return false;
    });

    //Checks the radiobutton value and returns if the project is approved or denied
    var isApproved = function () {

        var approved = true;
        var isSourceCodeCollected = $(".sourceCodeCollected:checked").val();
        var isShownRunningPjt = $(".runningPjt:checked").val();
        

        if (isSourceCodeCollected == "False" || isShownRunningPjt == "False" ) {
            approved = false;
        }

        return approved;
    }

});