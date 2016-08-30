//CUSTOMER => EFEEDBACJ.JS

$(function () {
    var form = $("#frmAdd");
    form.validate({
        errorPlacement: function errorPlacement(error, element) { element.before(error); }
    });
    form.find("#feedbackStepz").steps({
        headerTag: "h3",
        bodyTag: "section",
        transitionEffect: "slideLeft",
        onStepChanging: function (event, currentIndex, newIndex) {

            if (currentIndex > newIndex) {
                return true;
            }

            form.validate().settings.ignore = ":disabled,:hidden";
            return form.valid();

        },
        onFinishing: function (event, currentIndex) {

            form.validate().settings.ignore = ":disabled,:hidden";
            return form.valid();

        },
        onFinished: function (event, currentIndex) {


            if (validateRateYo()) {
                $(this).steps("previous");
                ajaxInsert();
            }
            else {
                return false;
            }


        }
    });

    //text-input to uppercase
    $(".toUpper").addClass('capitalise');

    //toastr options
    toastr.options = {
        "closeButton": true,
        "positionClass": "toast-bottom-right",
        "progressBar": true
    }

    $("#ddlInstructor").select2({
        placeholder: "Select  Center",
        allowClear: true
    });

    $("#ddlPreferredCourse").select2({
        placeholder: "Select  Course",
        allowClear: true
    });





    $("#chkbxHardCopyReqd").checkboxpicker({
        offClass: 'btn-primary',
        onClass: 'btn-primary',
        offLabel: 'NO',
        onLabel: 'YES'

    });

    //iCheck for checkbox and radio inputs
    $('input[type="radio"].minimal').iCheck({
        radioClass: 'iradio_square-blue',
        increaseArea: '20%'
    });

    $(".rateYo").each(function (e) {
        $(this).rateYo({

            onChange: function (rating, rateYoInstance) {

                var options = $(this).rateYo("option");
                var color = getColor(rating);
                /* set the 'ratedFill' of the plugin dynamically */
                $(this).rateYo("option", "ratedFill", color);
            },

            onSet: function (rating, rateYoInstance) {
                var nxtRateYoTextbox = $(this).parent('div').find('.rateYoText');
                var dislikeReasonDiv = $(this).parent('div').find('.dislikeReasonDiv');
                $(nxtRateYoTextbox).val(rating);
                if (rating <= 2 && rating != 0) {
                    $(dislikeReasonDiv).slideDown();
                }
                else {
                    $(dislikeReasonDiv).slideUp();
                }
            },


            fullStar: true
        })
    });


    //datepicker plugin
    $("#txtCourseStartDate").datepicker({
        autoclose: true
    }).on('changeDate', function (e) {
        $(e.target).valid();
        $("#txtCourseEndDate").val('');
        $("#txtCourseEndDate").datepicker('setStartDate', e.date);
    });;

    $("#txtCourseEndDate").datepicker({
        autoclose: true
    }).on('changeDate', function (e) {
        $(e.target).valid();

    });

    //On change of join status checkbox
    $(document).on("change", "#chkbxHardCopyReqd", function () {

        var required = $("#chkbxHardCopyReqd").is(':checked');

        if (required) {
            $("#divHardCopyCost").slideDown();
        }
        else {
            $("#divHardCopyCost").slideUp();
        }

    });

    //Load image on instructor change
    $(document).on("change", "#ddlInstructor", function (evt, params) {

        $.blockUI({ message: null });

        var currEmpId = $(this).val();
        var href = $("#divInstructor").data("url");

        if (currEmpId != "") {
            $(evt.target).valid();
            $.ajax({
                type: "POST",
                url: href,
                data: { empId: currEmpId },
                datatype: "json",
                success: function (data) {
                  
                    if (data.message != "error") {
                        var src = data.message;
                        src = src.replace("~", "..");
                        $("#imgInstructorPhoto").attr('src', src);

                    }
                    else {
                        $.unblockUI();
                        toastr.error("Cannot load the photo")
                    }
                },
                error: function (data) {
                    $.unblockUI();
                    toastr.error("Cannot load the photo")
                }
            });
        }
        else {
            $.unblockUI();
            var src = "~/UploadImages/Student/NoImageSelected.png";
            src = src.replace("~", "");
            $("#imgInstructorPhoto").attr('src', src);
        }


        return false;
    });

    var getColor = function (rating) {

        var color = "";
        if (rating == 1) {
            color = "#800000";
        }
        else if (rating == 2) {
            color = "#ff3300"
        }
        else if (rating == 3) {
            color = "#ff6600"
        }
        else if (rating == 4) {
            color = "#ffcc00"
        }
        else {
            color = "#ccff33"
        }

        return color;
    };


    $(document).on('ifChecked', '.single', function (event) {
        var futureCourseStatus = $(this).val();
        if (futureCourseStatus == "YES") {
            $(".futureCourseDiv").slideDown();
        }
        else {
            $(".futureCourseDiv").slideUp();
        }

    });

    var ajaxInsert = function () {



        $.blockUI({ message: '<h3><img src="../plugins/jQueryBlockUI/images/busy.gif" /> <b>Please wait... </b></h3>' });

        var $form = $("#frmAdd");
        var href = $form.data("url");
        var redirectUrl = $form.data("redirect-url");

        if ($form.valid()) {
            var formData = $($form).serialize();
            $.ajax({
                type: "POST",
                url: href,
                data: formData,
                datatype: "json",
                success: function (data) {

                    $.unblockUI();

                    window.location.href = redirectUrl;
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

    };


    var validateRateYo = function () {

        var validate = true;
        $(".rateYo").each(function (e) {

            var rating = $(this).rateYo("rating");
            if (rating == 0) {
                var missedRating = $(this).parent().parent().find('.control-label').first().text();

                bootbox.alert("Please rate <b>"+missedRating+" </b>");
                validate = false;
                return validate;
            }

        });
        return validate;
    }


    $('img').load(function () {
        $.unblockUI();
    });

});