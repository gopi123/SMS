
// Customer => LOGIN.JS
$(function () {

    //toastr options
    toastr.options = {
        "closeButton": true,
        "positionClass": "toast-bottom-right",
        "progressBar": true
    }

    $('#txtDOB').datepicker({
        autoclose: true
    });
      

   


    $(document).on("click", ".emailValidation", function () {
        bootbox.hideAll();
        $.blockUI({ message: null });

        var regNo = $("#txtStudentID").val();
        var href = $("#divModalRegister").data("email-validation");

        $.ajax({
            type: "GET",
            url: href,
            data: { regNo: regNo },
            datatype: "json",
            success: function (data) {
                $.unblockUI();

                var response = data.message;
                var fields = response.split('~');
                if (fields[0] == "success") {
                    bootbox.alert("Email send successfully to <b>" + fields[1] + "</b>.Please check your <b>Inbox/Spam/Junk</b> folder.");
                }
                else if (fields[0] == "error_regno") {
                    bootbox.alert("Invalid Student Id");
                }
                else {
                    bootbox.alert("Email cannot be send.");
                }
            },
            error: function (err) {
                toastr.error("Error:" + this.message)
            }
        });
        return false;
    })

    var showRegistration = function () {

        $('.form-control').val('');
        $('.errormessage').html('');
        $("#divModalRegister").modal({
            backdrop: 'static'
        });

        return false;
    }

    var SendRegistration = function () {
        var form = $("#frmMdlRegistration");
        var spinner = $('.spinner');
        var href = $(form).data('href');
        var studentID = $("#txtStudentID").val();
        var mobNo = $("#txtMobileNo").val();
        var email = $("#txtEmailID").val();
        var dob = $("#txtDOB").val();
        $(form).data('validator', null);
        $.validator.unobtrusive.parse($('form'))

        if (form.valid()) {


            spinner.show();


            $.ajax({
                type: "GET",
                url: href,
                data: { regNo: studentID, DOB: dob, mobNo: mobNo, email: email },
                datatype: "json",
                success: function (data) {
                    spinner.hide();

                    if (data.Status == "success") {
                        $('#divModalRegister').modal('hide');
                        bootbox.alert("A verification link has been sent to your registered email - <b>" + data.StudentEmail + "</b>.Kindly verify it to register.");
                    }
                    else if (data.Status == "email_not_verified") {
                        $('#divModalRegister').modal('hide');
                        bootbox.alert("Email not verifed yet.Kindly verify your emailid for registering.<br/>" +
                                       "If you haven't received your email yet <b><a href='' class='emailValidation'>Click Here</a> to validate</b>");

                    }
                    else if (data.Status == "studentid_not_present") {
                        $(".errormessage").html("Invalid StudentID");
                    }
                    else if (data.Status == "email_not_verified") {
                        $(".errormessage").html("EmailId not verified yet");
                    }
                    else if (data.Status == "student_already_registered") {
                        $(".errormessage").html("Student already registered");
                    }
                    else if (data.Status == "emailid_not_right") {
                        $(".errormessage").html("EmailId doesnot match with the Students EmailId");
                    }
                    else if (data.Status == "mobileno_not_right") {
                        $(".errormessage").html("Mobile Number does not match with the Students Mobile Number");
                    }
                    else if (data.Status == "dob_not_right") {
                        $(".errormessage").html("DOB doesnot match with the Students DOB");
                    }
                    else {
                        $(".errormessage").html("Error some thing happened");
                    }

                },
                error: function (err) {
                    spinner.hide();
                    $(".errormessage").html("Error:" + this.message)
                }
            });
        }

        return false;
    }

    var SaveRegistration = function () {
        var form = $("#frmAdd");
        var studentLoginId = $("#txtStudentLoginID").val();
        var userName = $("#txtUserName").val();
        var password = $("#txtPassword").val();
        var href = $(form).data("href");

        if (form.valid()) {

            $.blockUI({ message: '<h3><img src="../plugins/jQueryBlockUI/images/busy.gif" /> <b>Please wait... </b></h3>' });
            $.ajax({
                type: "POST",
                url: href,
                data: { studentLoginId: studentLoginId, userName: userName, password: password },
                datatype: "json",
                success: function (data) {
                    $.unblockUI();
                    if (data == "success") {
                        toastr.success("Successfully saved the details.")
                        setTimeout(function () {
                            var url = $(form).data("redirect-url");
                            window.location.href = url;
                        }, 2000);
                    }
                    else if (data == "student_already_registered") {
                        bootbox.alert("Student already registered");
                    }
                    else {
                        toastr.error("Error some thing gone wrong.")
                    }

                },
                error: function (err) {
                    toastr.error("Error:" + this.message)
                }
            });
        }

        return false;
    }

    var ShowForgotUsername = function () {
        $(".form-control").val("");
        $("#divModalForgotUsername").modal({
            backdrop: 'static'
        });
        return false;
    };

    var SendUsername = function () {
        var mobNo = $("#txtMobileNo_Username").val();
        var emailId = $("#txtEmailID_Username").val();
        var studentID = $("#txtStudentID_Username").val();
        var href = $("#divModalForgotUsername").data("url");
        var divErrorMsg = $(".submit-error-message");
        var spinner = $(this).parent("div").find(".spinner");

        if (mobNo != "" && emailId != "" && studentID != "") {
            $(spinner).show();
            $.ajax({
                type: "GET",
                url: href,
                data: { mobNo: mobNo, emailId: emailId, studentID: studentID },
                datatype: "json",
                success: function (data) {
                    $(spinner).hide();
                    if (data.Status == "success") {
                        $('#divModalForgotUsername').modal('hide');
                        bootbox.alert("Username has been sent to your emailid-<b>" + data.StudentEmail + "</b>.Kindly check.")
                    }
                    else if (data.Status == "student_not_present") {
                        $(divErrorMsg).html("Student not found.Kindly enter valid EmailId and MobileNo.");
                        setTimeout(function () {
                            $(divErrorMsg).html("");
                        }, 3000);
                    }
                    else {
                        toastr.error("Error some thing gone wrong.")
                    }

                },
                error: function (err) {
                    $(spinner).hide();
                    toastr.error("Error:" + this.message)
                }
            });
        }
        else {
            if (emailId == "") {
                var spanDiv_email = $("#txtEmailID_Username").parent('div').find('.error-message');
                $(spanDiv_email).html("Enter Username");
                setTimeout(function () {
                    $(spanDiv_email).html("");
                }, 2000);



            }
            if (mobNo == "") {
                var spanDiv_mobNo = $("#txtMobileNo_Username").parent('div').find('.error-message');
                $(spanDiv_mobNo).html("Enter MobileNo");
                setTimeout(function () {
                    $(spanDiv_mobNo).html("");
                }, 2000);


            }
            if (studentID == "") {
                var spanDiv_studentID = $("#txtStudentID_Username").parent('div').find('.error-message');
                $(spanDiv_studentID).html("Enter StudentID");
                setTimeout(function () {
                    $(spanDiv_studentID).html("");
                }, 2000);


            }
        }
        return false;
    }

    var ShowForgotPassword = function () {
        $(".form-control").val("");
        $("#divModalForgotPassword").modal({
            backdrop: 'static'
        });
        return false;
    }

    var SendPassword = function () {
        var mobNo = $("#txtMobileNo_Password").val();
        var emailId = $("#txtEmailID_Password").val();
        var studentID = $("#txtStudentID_Password").val();
        var href = $("#divModalForgotPassword").data("url");
        var divErrorMsg = $(".password-error-message");
        var spinner = $(this).parent("div").find(".spinner");

        if (mobNo != "" && emailId != "" && studentID != "") {

            //$.blockUI({ message: '<h3><img src="../plugins/jQueryBlockUI/images/busy.gif" /> <b>Please wait... </b></h3>' });
            $(spinner).show();
            $.ajax({
                type: "GET",
                url: href,
                data: { mobNo: mobNo, emailId: emailId, studentID: studentID },
                datatype: "json",
                success: function (data) {
                    //$.unblockUI();
                    $(spinner).hide();
                    if (data.Status == "success") {
                        $('#divModalForgotPassword').modal('hide');
                        bootbox.alert("A link has been send to your EmailId-<b>" + data.StudentEmail + "</b>.Kindly verify it to reset password");
                    }
                    else if (data.Status == "student_not_present") {
                        $(divErrorMsg).html("Student not found.Kindly enter valid EmailId and MobileNo.");
                        setTimeout(function () {
                            $(divErrorMsg).html("");
                        }, 3000);
                    }
                    else {
                        toastr.error("Error some thing gone wrong.")
                    }

                },
                error: function (err) {
                    $(spinner).hide();
                    toastr.error("Error:" + this.message)
                }
            });
        }
        else {
            if (emailId == "") {
                var spanDiv_email = $("#txtEmailID_Password").parent('div').find('.error-message');
                $(spanDiv_email).html("Enter Username");
                setTimeout(function () {
                    $(spanDiv_email).html("");
                }, 2000);



            }
            if (mobNo == "") {
                var spanDiv_mobNo = $("#txtMobileNo_Password").parent('div').find('.error-message');
                $(spanDiv_mobNo).html("Enter MobileNo");
                setTimeout(function () {
                    $(spanDiv_mobNo).html("");
                }, 2000);


            }
            if (studentID == "") {
                var spanDiv_studentID = $("#txtStudentID_Password").parent('div').find('.error-message');
                $(spanDiv_studentID).html("Enter Student ID");
                setTimeout(function () {
                    $(spanDiv_studentID).html("");
                }, 2000);


            }
        }



        return false;
    }

    var SaveForgotPassword = function () {
        var form = $("#frmAdd");
        var studentLoginId = $("#txtStudentLoginID").val();
        var password = $("#txtPassword").val();
        var href = $(form).data("href");

        if (form.valid()) {

            $.blockUI({ message: '<h3><img src="../plugins/jQueryBlockUI/images/busy.gif" /> <b>Please wait... </b></h3>' });
            $.ajax({
                type: "POST",
                url: href,
                data: { studentLoginId: studentLoginId, password: password },
                datatype: "json",
                success: function (data) {
                    $.unblockUI();
                    if (data.Status == "success") {
                        toastr.success("Successfully saved the details.")
                        setTimeout(function () {
                            var url = $(form).data("redirect-url");
                            window.location.href = url;
                        }, 2000);
                    }
                    else {
                        toastr.error("Error some thing gone wrong.")
                    }

                },
                error: function (err) {
                    toastr.error("Error:" + this.message)
                }
            });
        }

        return false;
    }

    $(document).on("click", "#lbtnRegister", showRegistration);

    $(document).on("click", "#btnSendRegistration", SendRegistration);

    $(document).on("click", "#btnRegister", SaveRegistration);

    $(document).on("click", "#lbtnUserName", ShowForgotUsername);

    $(document).on("click", "#btnForgotUsername", SendUsername);

    $(document).on("click", "#lbtnPassword", ShowForgotPassword);

    $(document).on("click", "#btnForgotPassword", SendPassword);

    $(document).on("click", "#btnSavePassword", SaveForgotPassword);
});