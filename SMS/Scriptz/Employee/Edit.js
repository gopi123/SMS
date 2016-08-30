//EMPLOYEE ==> EDIT.JS

$(function () {
    //Steps plugin jquery

    var form = $("#frmEdit");
    form.validate({
        errorPlacement: function errorPlacement(error, element) { element.before(error); }
    });
    form.find("#employeeStepz").steps({
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

            $(this).steps("previous");
            $(this).steps("previous");
            ajaxUpdate();

        }
    });


    //text-input to uppercase
    $(".toUpper").addClass('capitalise');

    $("#ddlCenterCode").select2({
        placeholder: "Select  Center",
        allowClear: true,
        width: 400

    });

    $("#ddlState").select2({
        placeholder: "Select  State",
        allowClear: true


    });

    $("#ddlDistrict").select2({
        placeholder: "Select  District",
        allowClear: true

    });

    $("#ddlQlfnType").select2({
        placeholder: "Select  Qualification",
        allowClear: true,
        width: 400
    });

    $("#ddlQlfnMain").select2({
        placeholder: "Select  Course",
        allowClear: true,
        width: 400
    });

    $("#ddlQlfnSub").select2({
        placeholder: "Select  Stream",
        allowClear: true,
        width: 400
    });

    $("#ddlDepartment").select2({
        placeholder: "Select  Department",
        allowClear: true,
        width: 400
    });

    $("#ddlDesignation").select2({
        placeholder: "Select  Designation",
        allowClear: true,
        width: 400
    });

    //Checkbox switch function
    $("#chkbxGender").checkboxpicker({
        offClass: 'btn-primary',
        onClass: 'btn-primary',
        offLabel: 'MALE',
        onLabel: 'FEMALE'
    });

    //Checkbox switch function
    $("#chkbxMaritalStatus").checkboxpicker({
        offClass: 'btn-primary',
        onClass: 'btn-primary',
        offLabel: 'UNMARRIED',
        onLabel: 'MARRIED'
    });

    
    //datepicker plugin
    $("#txtDOB,#txtDateOfJoin").datepicker({
        autoclose: true
    });

    //NumberOnly textbox
    $(document).on("keypress", ".numberOnly", function (evt) {
        evt = (evt) ? evt : window.event;
        var charCode = (evt.which) ? evt.which : evt.keyCode;
        if (charCode > 31 && (charCode < 48 || charCode > 57)) {
            return false;
        }
        return true;
    });

    //inputmask    
    $("#txtDOB,#txtDateOfJoin").inputmask("mm/dd/yyyy", { "placeholder": "mm/dd/yyyy" })

    $("#txtMobileNo,#txtOfficialMobileNo").inputmask("9999999999");

    $("#txtPincode").inputmask("999999");

    //toastr options
    toastr.options = {
        "closeButton": true,
        "positionClass": "toast-bottom-right",
        "progressBar": true
    }

    //stop showing validation if someitem is selected
    $("#ddlState,#ddlDistrict,#ddlCenterCode,#ddlDepartment,#ddlDesignation,#ddlQlfnType,#ddlQlfnMain").change(function (evt, params) {
        if ($(this).val() != "" && $(this).val() != null) {
            $(evt.target).valid();
        }
    });

    //isLoading function
    var isLoading = function (loading) {
        if (loading) {
            $('.content').loading({
                message: "Please wait ...",
                theme: 'dark'
            });
        }
        else {
            $('.content').loading('stop');
        }
    };

    //image type checking
    $('.imgUpload').on('fileloaded', function (event, previewId) {
        var id = $(this).attr("id");
        var ext = $(this).val().split('.').pop().toLowerCase();
        if (ext == 'jpg' || ext == 'jpeg' || ext == 'png' || ext == 'gif') {
            return true
        }
        else {
            bootbox.alert("You can upload only files of type <strong>jpg ,jpeg ,png ,gif </strong>", function () {
                $("#" + id).fileinput('clear');
            });

        }
    });


    var GetDistrict = function () {

        var stateId = $("#ddlState").val();
        var spinner = $(this).parent('div').find('.spinner');
        var $ddlDistrict = $("#ddlDistrict");
        var href = $("#divDistrict").data("url");

        if (stateId != "") {

            $(spinner).toggle(true);

            $.ajax({
                type: "GET",
                url: href,
                data: { stateId: stateId },
                datatype: "json",
                success: function (data) {
                    $(spinner).toggle(false);
                    $ddlDistrict.html('');//clearing the dpdwn html
                    $ddlDistrict.select2("val", "");//resetting dpdwn for clearing the selected option
                    $ddlDistrict.append('<option></option>')
                    $.each(data, function () {
                        $ddlDistrict.append($('<option></option>').val(this.Id).html(this.Name));
                    });
                },
                error: function (err) {
                    $(spinner).toggle(false);
                    toastr.error("Error:" + this.message)
                }
            });
        }
        else {
            $ddlDistrict.html('');//clearing the dpdwn html
            $ddlDistrict.select2("val", "");//resetting dpdwn for clearing the selected option
        }

    }

    var GetDesignation = function () {

        var departmentId = $("#ddlDepartment").val();
        var spinner = $(this).parent('div').find('.spinner');
        var $ddlDesignation = $("#ddlDesignation");
        var href = $("#divDesignation").data("url");

        if (departmentId != "") {

            $(spinner).toggle(true);

            $.ajax({
                type: "GET",
                url: href,
                data: { departmentId: departmentId },
                datatype: "json",
                success: function (data) {
                    $(spinner).toggle(false);
                    $ddlDesignation.html('');//clearing the dpdwn html
                    $ddlDesignation.select2("val", "");//resetting dpdwn for clearing the selected option
                    $ddlDesignation.append('<option></option>')
                    $.each(data, function () {
                        $ddlDesignation.append($('<option></option>').val(this.Id).html(this.Name));
                    });
                },
                error: function (err) {
                    $(spinner).toggle(false);
                    toastr.error("Error:" + this.message)
                }
            });
        }
        else {
            $ddlDesignation.html('');//clearing the dpdwn html
            $ddlDesignation.select2("val", "");//resetting dpdwn for clearing the selected option
        }

    }

    var GetCourse = function () {

        var qlfnTypeId = $("#ddlQlfnType").val();
        var spinner = $(this).parent('div').find('.spinner');
        var $ddlQlfnMain = $("#ddlQlfnMain");
        var href = $("#divCourse").data("url");

        if (qlfnTypeId != "") {

            $(spinner).toggle(true);

            $.ajax({
                type: "GET",
                url: href,
                data: { qlfnTypeId: qlfnTypeId },
                datatype: "json",
                success: function (data) {
                    $(spinner).toggle(false);
                    $ddlQlfnMain.html('');//clearing the dpdwn html
                    $ddlQlfnMain.select2("val", "");//resetting dpdwn for clearing the selected option
                    $ddlQlfnMain.append('<option></option>')
                    $.each(data, function () {
                        $ddlQlfnMain.append($('<option></option>').val(this.Id).html(this.Name));
                    });
                },
                error: function (err) {
                    $(spinner).toggle(false);
                    toastr.error("Error:" + this.message)
                }
            });
        }
        else {
            $ddlQlfnMain.html('');//clearing the dpdwn html
            $ddlQlfnMain.select2("val", "");//resetting dpdwn for clearing the selected option
        }

    }

    var GetStream = function () {

        var qlfnMainId = $("#ddlQlfnMain").val();
        var spinner = $(this).parent('div').find('.spinner');
        var $ddlQlfnSub = $("#ddlQlfnSub");
        var href = $("#divStream").data("url");

        if (qlfnMainId != "" && qlfnMainId != null) {

            $(spinner).toggle(true);

            $.ajax({
                type: "GET",
                url: href,
                data: { qlfnMainId: qlfnMainId },
                datatype: "json",
                success: function (data) {
                    $(spinner).toggle(false);
                    $ddlQlfnSub.html('');//clearing the dpdwn html
                    $ddlQlfnSub.select2("val", "");//resetting dpdwn for clearing the selected option
                    $ddlQlfnSub.append('<option></option>')
                    $.each(data, function () {
                        $ddlQlfnSub.append($('<option></option>').val(this.Id).html(this.Name));
                    });
                },
                error: function (err) {
                    $(spinner).toggle(false);
                    toastr.error("Error:" + this.message)
                }
            });
        }
        else {
            $ddlQlfnSub.html('');//clearing the dpdwn html
            $ddlQlfnSub.select2("val", "");//resetting dpdwn for clearing the selected option
        }

    }

    //ajax insert
    var ajaxUpdate = function () {
        var form = $("#frmEdit");
        var redirectUrl = $(form).data("redirect-url");
        $("#frmEdit").ajaxSubmit({
            iframe: true,
            dataType: "json",
            beforeSubmit: function () {
                isLoading(true);

            },
            success: function (result) {
                isLoading(false);               
                if (result.message == "success") {
                    toastr.success("Successfully updated the details.");
                    setTimeout(function () {
                        window.location.href = redirectUrl;
                    }, 2000);
                }
                else {
                    toastr.error("Error:Something gone wrong")
                }
            },
            error: function (xhr, textStatus, errorThrown) {
                isLoading(false);
                toastr.error("Error:Something gone wrong")
                //$("#ajaxUploadForm").unblock();
                //$("#ajaxUploadForm").resetForm();
                //$.growlUI(null, 'Error uploading file');
            }

        });
    }

    //Get District on State Change
    $(document).on("change", "#ddlState", GetDistrict);

    //Get Designation on Department Change
    $(document).on("change", "#ddlDepartment", GetDesignation);

    //Get Course on Qualification Change
    $(document).on("change", "#ddlQlfnType", GetCourse);

    //Get Stream on Course Change
    $(document).on("change", "#ddlQlfnMain", GetStream);

});