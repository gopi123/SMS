//WALKINN ==> EDIT.JS
$(function () {
    var form = $("#frmEdit");
    form.validate({
        errorPlacement: function errorPlacement(error, element) { element.before(error); }
    });
    form.find("#WalkInnStepz").steps({
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
            if (form.valid()) {
                //Qlfn Tab
                if (currentIndex == 1) {
                    return ValidateCustomerTypeDivs();
                }

                if (currentIndex == 2) {
                    return ValidateReference();
                }
                return form.valid();
            }
            return form.valid();
        },
        onFinishing: function (event, currentIndex) {
            $(form).data('validator', null);
            $.validator.unobtrusive.parse($('form'));
            form.validate().settings.ignore = ":disabled,:hidden";
            if (form.valid()) {
                return ValidateOtherDetails();
            }
            return form.valid();
        },
        onFinished: function (event, currentIndex) {

            $(this).steps("previous");
            $(this).steps("previous");
            $(this).steps("previous");
            ajaxEdit();
        }
    });



    //text-input to uppercase
    $(".form-control").addClass('capitalise');

    //iCheck for checkbox and radio inputs
    $('input[type="radio"].minimal').iCheck({
        radioClass: 'iradio_square-blue',
        increaseArea: '20%'
    });

    //toastr options
    toastr.options = {
        "closeButton": true,
        "positionClass": "toast-bottom-right",
        "progressBar": true
    }

    //NumberOnly textbox
    $(document).on("keypress", ".numberOnly", function (evt) {
        evt = (evt) ? evt : window.event;
        var charCode = (evt.which) ? evt.which : evt.keyCode;
        if (charCode > 31 && (charCode < 48 || charCode > 57)) {
            return false;
        }
        return true;
    });

    $("#txtPincode").inputmask("999999");

    $(".mobileValidate").inputmask("9999999999");

    $("#txtDOB").inputmask("mm/dd/yyyy", { "placeholder": "mm/dd/yyyy" })


    //Checkbox picker
    $("#chkbxGender").checkboxpicker({
        offClass: 'btn-primary',
        onClass: 'btn-primary',
        offLabel: 'MALE',
        onLabel: 'FEMALE'

    });

    $("#chkbxGuardian").checkboxpicker({
        offClass: 'btn-primary',
        onClass: 'btn-primary',
        offLabel: 'FATHER',
        onLabel: 'SPOUSE'

    });

    $("#chkbxPrevExp,#chkbxDemo,#chkbxJoinStatus").checkboxpicker({
        offClass: 'btn-danger',
        onClass: 'btn-success',
        offLabel: 'NO',
        onLabel: 'YES',
        reverse: true

    });




    //datepicker plugin
    $("#txtDOB,#txtJoinDate").datepicker({
        autoclose: true,
        onSelect: function (dateText, inst) {
            $(this).change();
        }
    }).on("change", function (evt) {
        if ($(this).val() != "" && $(this).val() != null) {
            $(evt.target).valid();
        }
    });



    //format month and year only
    $("#txtPrvExpYear,#txtJoinDetails").datepicker({
        format: "MM-yyyy",
        startView: "months",
        minViewMode: "months",
        autoclose: true,
        onSelect: function (dateText, inst) {
            $(this).change();
        }
    }).on("change", function (evt) {
        if ($(this).val() != "" && $(this).val() != null) {
            $(evt.target).valid();
        }
    });

    //format year only
    $("#txtYearCompletion").datepicker({
        format: "yyyy",
        startView: "years",
        minViewMode: "years",
        autoclose: true
    });

    //select2 plugin
    $("#ddlState").select2({
        placeholder: "Select  State",
        allowClear: true

    });

    $("#ddlDistrict").select2({
        placeholder: "Select  District",
        allowClear: true
    });

    $("#ddlQlfnMain").select2({
        placeholder: "Select  Course",
        allowClear: true


    });

    $("#ddlQlfnType").select2({
        placeholder: "Select  Qualification",
        allowClear: true


    });

    $("#ddlQlfnSub").select2({
        placeholder: "Select  Stream",
        allowClear: true


    });

    $("#ddlCustomerType").select2({
        placeholder: "Select  Customer Type",
        allowClear: true

    });

    $("#ddlCenterCode").select2({
        placeholder: "Select  Center",
        allowClear: true

    });




    $("#ddlKnowHow").select2({
        placeholder: "Select Details",
        allowClear: true

    });

    $("#ddlPlacement").select2({
        placeholder: "Select Placement",
        allowClear: true


    });

    $("#ddlWhyNS").select2({
        placeholder: "Select Details",
        allowClear: true


    });

    $("#ddlCRO1").select2({
        placeholder: "Select Employee",
        allowClear: true
    });


    $("#ddlCRO2").select2({
        placeholder: "Select Employee",
        allowClear: true
    });


    $("#ddlEquipmentDemo").select2({
        placeholder: "Select Employee",
        allowClear: true


    });

    $("#ddlCourseList").select2({
        placeholder: "Select Course",
        allowClear: true


    });

    $("#ddlBatchPreferred").select2({
        placeholder: "Select Batch",
        allowClear: true


    });


    $("#ddlCareGiver").select2({
        placeholder: "Select Care Giver",
        allowClear: true


    });

    $("#ddlCurrentYear").select2({
        placeholder: "Select Current Year",
        allowClear: true


    });


    $("#ddlCenter").select2({
        placeholder: "Select WalkInn Center",
        allowClear: true


    });
    //stop showing validation if someitem is selected
    $("#ddlState,#ddlDistrict,#ddlQlfnType,#ddlQlfnMain,#ddlQlfnSub,#ddlCustomerType,#ddlKnowHow,#ddlPlacement,#ddlWhyNS,#ddlProspectHandled,#ddlEquipmentDemo,#ddlCourseList,#ddlCareGiver,#ddlCurrentYear").change(function (evt, params) {
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

    //shows customer status div based on custommertype value
    var ShowCustomerTypeDiv = function () {

        var customerTypeId = $("#ddlCustomerType").val();
        var qlfnId = $("#ddlQlfnType").val();
        if (customerTypeId == 1) {
            if (qlfnId == 4) {
                $('#ddlCurrentYear').attr('disabled', 'disabled');
            }
            $("#divCurrentYear").show();
            $("#divCollege").show();
        }
        else if (customerTypeId == 2) {
            $("#divYearCompletion").show();
            $("#divCollege").show();
        }
        else {
            $(".employ").show();
        }
    }

    //Hide/Show preivous center details based on previous experience change
    var ShowCenter = function () {
        var result = $("#chkbxPrevExp").is(':checked');
        if (result) {
            $(".prevExp").slideDown();

        }
        else {
            $(".prevExp").slideUp();
        }

        return false;
    }

    //Hide/Show demo details based on previous experience change
    var ShowDemo = function () {
        var result = $("#chkbxDemo").is(':checked');
        if (result) {

            $(".demogiven").slideDown();
        }
        else {
            $(".demogiven").slideUp();
        }

        return false;
    }

    //Hide/Show batch details based on previous experience change
    var ShowBatch = function () {
        var result = $("#chkbxJoinStatus").is(':checked');
        if (result) {
            $(".joinreason").slideDown();
            $(".notjoinreason").slideUp();
        }
        else {
            $(".joinreason").slideUp();
            $(".notjoinreason").slideDown();
        }

        return false;
    }

    //Slideup divs if stream is not selected
    var ValidateCustomerType = function () {

        var $ddlCustomerType = $("#ddlCustomerType");
        var streamId = $("#ddlQlfnSub").val();
        if (streamId == "") {
            $ddlCustomerType.select2("val", "");
            $('#divCurrentYear').slideUp();
            $('#divYearCompletion').slideUp();
            $('#divCollege').slideUp();
            $('#divExperience').slideUp();
            $('#divIndustry').slideUp();
            $('#divCompany').slideUp();
            return false;
        }
    }

    //Get District on State change
    var GetDistrict = function () {

        var stateId = $("#ddlState").val();
        var spinner = $(this).parent('div').parent('div').find('.spinner');
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

    //Get Course on Qlfn change
    var GetCourse = function () {

        var qlfnTypeId = $("#ddlQlfnType").val();
        var spinner = $(this).parent('div').find('.spinner');
        var $ddlQlfnMain = $("#ddlQlfnMain");
        var href = $("#divCourse").data("url");

        if (qlfnTypeId != "") {

            $(spinner).toggle(true);

            //If qualification is schooling
            if (qlfnTypeId == 4) {
                $('#ddlCurrentYear').select2("val", "");
                $('#ddlCurrentYear').attr('disabled', 'disabled');
            }
            else {
                $('#ddlCurrentYear').removeAttr('disabled');
            }

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

    //Get stream on course change
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

    //Hide/Show Divs based on customer type change
    var showDiv = function () {

        var $ddlCustomerType = $("#ddlCustomerType");
        var customerType = $("#ddlCustomerType").val();
        if (customerType != "") {
            //Gets the value of stream
            var streamId = $("#ddlQlfnSub").val();
            //if stream is not selected
            if (streamId == "") {
                bootbox.alert("Please select stream");
                $ddlCustomerType.select2("val", "");//resetting dpdwn for clearing the selected option
                $('#divCurrentYear').attr('style', 'display: none');
                return false;
            }

            //For Student
            if (customerType == 1) {
                $(".student").slideDown();
                $("#divCollege").slideDown();
                $(".nonemploy").slideUp();
                $(".employ").slideUp();
            }
                //For Non Employed
            else if (customerType == 2) {
                $(".nonemploy").slideDown();
                $("#divCollege").slideDown();
                $(".student").slideUp();
                $(".employ").slideUp();
            }
                //For Employed
            else {
                $(".employ").slideDown();
                $(".nonemploy").slideUp();
                $("#divCollege").slideUp();
                $(".student").slideUp();

            }
        }
        return false;
    };

    //Validate customer type divs based on its selection
    var ValidateCustomerTypeDivs = function () {

        var customerType = $("#ddlCustomerType").val();
        var qlfnTypeId = $("#ddlQlfnType").val();
        var currYear = $("#ddlCurrentYear").val();
        var yearOfCompletion = $("#txtYearCompletion").val();
        var collegeAddress = $("#txtCollegeAddress").val();
        var expInYears = $("#txtExp").val();
        var industryType = $("#txtIndustryType").val();
        var companyAddress = $("#txtCompany").val();
        var validate = true;

        if (customerType == 1) {
            //If qlfn is not schooling
            if (qlfnTypeId != 4) {
                if (currYear == "") {
                    validate = false;
                    bootbox.alert("Please select <strong> Current Year </strong>.");
                    $("#ddlCurrentYear").focus();
                    return false;
                };
                if (collegeAddress == "") {
                    validate = false;
                    bootbox.alert("Please enter <strong> Institution Address </strong>.");
                    $("#txtCollegeAddress").focus();
                    return false;
                };

            }
                //if qlfn is schooling 
            else {
                if (collegeAddress == "") {
                    validate = false;
                    bootbox.alert("Please select <strong>Institution Name and Address </strong>.");
                    $("#txtCollegeAddress").focus();
                    return false;
                };
            }
        }
        else if (customerType == 2) {
            if (yearOfCompletion == "") {
                validate = false;
                bootbox.alert("Please select <strong> Year of Completion </strong>.");
                $("#txtYearCompletion").focus();
                return false;
            };
            if (collegeAddress == "") {
                validate = false;
                bootbox.alert("Please enter  <strong>Institution Name and Address </strong>.");
                $("#txtCollegeAddress").focus();
                return false;
            };
        }
        else {
            if (expInYears == "") {
                validate = false;
                bootbox.alert("Please enter <strong>Experience in Years </strong >.");
                $("#txtExp").focus();
                return false;
            };
            if (industryType == "") {
                validate = false;
                bootbox.alert("Please enter <strong> Type Of Industry </strong>.");
                $("#txtIndustryType").focus();
                return false;
            };
            if (companyAddress == "") {
                validate = false;
                bootbox.alert("Please enter <strong> Company Address </strong>.");
                $("#txtCompany").focus();
                return false;
            };
        }

        return validate;
    }

    //Validate Other details divs based on its selection
    var ValidateOtherDetails = function () {
        var validate = true;
        var isDemoGiven = $("#chkbxDemo").is(':checked');
        var joinStatus = $("#chkbxJoinStatus").is(':checked');
        var isTrainingTaken = $("#chkbxPrevExp").is(':checked');
        var cro1Id = $("#ddlCRO1").val();
        var cro2Id = $("#ddlCRO2").val();

        //Place of training validation is performed here
        if (isTrainingTaken) {
            var prevExpPlace = $("#txtPrvExpPlace").val();
            var prevExpYear = $("#txtPrvExpYear").val();
            var prevExpTrainer = $("#txtPrvExpTrainer").val();

            if (prevExpPlace == "") {
                validate = false;
                bootbox.alert("Please enter <strong> Place of training</strong>.");
                $("#txtPrvExpPlace").focus();
                return false;
            }

            if (prevExpYear == "") {
                validate = false;
                bootbox.alert("Please enter <strong> Year of training</strong>.");
                $("#txtPrvExpYear").focus();
                return false;
            }

            if (prevExpTrainer == "") {
                validate = false;
                bootbox.alert("Please enter <strong> Name of trainer</strong>.");
                $("#txtPrvExpTrainer").focus();
                return false;
            }
        }

        //Demogiven validation is performed here
        if (isDemoGiven) {
            var demoEmp = $("#ddlEquipmentDemo").val();
            var feedBack = $("#txtFeedback").val();
            if (demoEmp == "") {
                validate = false;
                bootbox.alert("Please select <strong> Demo Given By</strong>.");
                $("#ddlEquipmentDemo").focus();
                return false;
            }
            if (feedBack == "") {
                validate = false;
                bootbox.alert("Please enter <strong> Customer Feedback about demo</strong>.");
                $("#txtFeedback").focus();
                return false;
            }
        }
        //JoinStatus validation is performed here
        if (joinStatus) {
            var joinDate = $("#txtJoinDate").val();
            var batchPrefferred = $("#ddlBatchPreferred").val();
            if (joinDate == "") {
                validate = false;
                bootbox.alert("Please select <strong> Expected joining date </strong>.");
                $("#txtJoinDate").focus();
                return false;
            }
            if (batchPrefferred == "") {
                validate = false;
                bootbox.alert("Please select <strong> Batch Preferred </strong>.");
                $("#ddlBatchPreferred").focus();
                return false;
            }
        }
        else {
            var notJoiningReason = $("#txtNotJoiningReason").val();
            if (notJoiningReason == "") {
                validate = false;
                bootbox.alert("Please select <strong> Not Joining Reason </strong>.");
                $("#txtNotJoiningReason").focus();
                return false;
            }
        }

        //CROChecking is performed here
        if (cro1Id == cro2Id) {
            validate = false;
            bootbox.alert("CRO1 Name and CRO2 Name cannot be same");
            return false;
        }

        return validate;
    }

    //Initializing datatable relation
    var InitRelationTable = function () {

        var rowCount = $('#tblStudentRelation >tbody >tr').length;

        //IF NO RELATION IS ADDED     
        if (rowCount == 0) {

            var $tbody = $("#tblStudentRelation tbody");
            $($tbody).html('');

            //Adds 5 rows to the datatable
            for (var i = 0; i < 5; i++) {

                var slno = parseInt(i + 1);

                var $row = $('<tr/>');
                $row.append(' <td class="slno">' + slno + '</td>');
                $row.append(' <td><input name="StudentRelation[' + i + '].Name" type="text" class="form-control capitalise refName"  placeholder="Name" /></td>');

                $row.append(' <td><input name="StudentRelation[' + i + '].Relation" type="text" class="form-control capitalise refRelation"  placeholder="Relation" /></td>');

                $row.append(' <td><input name="StudentRelation[' + i + '].EmailId" type="text" class="form-control capitalise refEmailId" data-val="true"  data-val-email="Invalid EmailId"  placeholder="EmailId" />' +
                    '<span class="field-validation-valid spanEmailId" data-valmsg-for="StudentRelation[' + i + '].EmailId" data-valmsg-replace="true"></span></td>');

                $row.append(' <td><input name="StudentRelation[' + i + '].MobileNo" type="text" class="form-control numberOnly refMobile" data-val="true" placeholder="Contact No" /></td>');
                $tbody.append($row);
            }

        }
            //IF RELATION IS ADDED BUT ITS COUNT IS LESS THAN 5
        else if (rowCount < 5) {
            var $tbody = $("#tblStudentRelation tbody");

            for (var i = rowCount; i < 5; i++) {

                var slno = parseInt(i + 1);

                var $row = $('<tr/>');
                $row.append(' <td class="slno">' + slno + '</td>');
                $row.append(' <td><input name="StudentRelation[' + i + '].Name" type="text" class="form-control capitalise refName"  placeholder="Name" /></td>');

                $row.append(' <td><input name="StudentRelation[' + i + '].Relation" type="text" class="form-control capitalise refRelation"  placeholder="Relation" /></td>');

                $row.append(' <td><input name="StudentRelation[' + i + '].EmailId" type="text" class="form-control capitalise refEmailId" data-val="true"  data-val-email="Invalid EmailId"  placeholder="EmailId" />' +
                    '<span class="field-validation-valid spanEmailId" data-valmsg-for="StudentRelation[' + i + '].EmailId" data-valmsg-replace="true"></span></td>');

                $row.append(' <td><input name="StudentRelation[' + i + '].MobileNo" type="text" class="form-control numberOnly refMobile" data-val="true" placeholder="Contact No" /></td>');
                $tbody.append($row);
            }

        }

    }

    //Validate Reference on AddRow Click + Next Button
    var ValidateReference = function () {
        var errorCount = 0;
        $("#tblStudentRelation tbody tr").each(function (i, el) {
            //Gets the reference name of each row
            var $tdRefName = $(this).find('.refName');
            ///Gets the reference relation of each row
            var $tdRefRelation = $(this).find('.refRelation');
            //Gets the reference mobile of each row
            var $tdRefMobile = $(this).find('.refMobile');
            //Gets the reference mobile of each row
            var $tdRefEmail = $(this).find('.refEmailId');

            if ($tdRefName.val() != "" || $tdRefRelation.val() != "" || $tdRefMobile.val() != "" || $tdRefEmail.val() != "") {
                if ($tdRefName.val() == "") {
                    errorCount++;
                    var $td = $($tdRefName).parent('td');
                    $($td).append("<span class='reference-error field-validation-error'>Enter Name </span>")
                    setTimeout(function () {
                        $('.reference-error').fadeOut(1500);

                    }, 3000);

                }
                if ($tdRefRelation.val() == "") {
                    errorCount++;
                    var $td = $($tdRefRelation).parent('td');
                    $($td).append("<span class='reference-error field-validation-error'>Enter Relation </span>")
                    setTimeout(function () {
                        $('.reference-error').fadeOut(1500);

                    }, 3000);
                }
                if ($tdRefMobile.val() == "") {
                    errorCount++;
                    var $td = $($tdRefMobile).parent('td');
                    $($td).append("<span class='reference-error field-validation-error'>Enter ContactNo </span>")
                    setTimeout(function () {
                        $('.reference-error').fadeOut(1500);

                    }, 3000);
                }
            }


        });
        if (errorCount > 0) {
            return false;
        }
        else {
            return true;
        }

    }

    //Adds row to datatable on addrow click
    var AddDataTableRow = function () {

        var rowCount = $('#tblStudentRelation >tbody >tr').length;

        var slno = parseInt(rowCount + 1);
        var i = parseInt(rowCount);

        var $row = $('<tr/>');
        $row.append(' <td class="slno">' + slno + '</td>');
        $row.append(' <td><input name="StudentRelation[' + i + '].Name" type="text" class="form-control capitalise refName"  placeholder="Name" /></td>');

        $row.append(' <td><input name="StudentRelation[' + i + '].Relation" type="text" class="form-control capitalise refRelation"  placeholder="Relation" /></td>');

        $row.append(' <td><input name="StudentRelation[' + i + '].EmailId" type="text" class="form-control capitalise refEmailId" data-val="true"  data-val-email="Invalid EmailId"  placeholder="EmailId" />' +
            '<span class="field-validation-valid spanEmailId" data-valmsg-for="StudentRelation[' + i + '].EmailId" data-valmsg-replace="true"></span></td>');

        $row.append(' <td><input name="StudentRelation[' + i + '].MobileNo" type="text" class="form-control numberOnly refMobile" data-val="true"  placeholder="Contact No" /></td>');

        $row.append('<td><div class="tools"><a class="btn rowDelete"><i class="fa  fa-trash-o " style="color:red"></i></a></div></td>');

        $("#tblStudentRelation tbody tr:last").after($row);
    }

    //ajax insert
    var ajaxEdit = function () {

        var form = $("#frmEdit");
        var redirectUrl = $(form).data("redirect-url");
        if (form.valid()) {

            isLoading(true);
            var href = $(form).data('url');
            var formData = $(form).serialize();
            $.ajax({
                type: "POST",
                url: href,
                data: formData,
                datatype: "json",
                success: function (data) {
                    isLoading(false);
                    if (data.message == "success") {
                        //clearAll();
                        toastr.success("Successfully updated the details.")
                        setTimeout(function () {
                            window.location.href = redirectUrl;
                        }, 2000);
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

    //Show CRODiv based on radiobutton checked
    var ShowCRODiv = function () {
        //Gets the value of checked radiobutton
        var val = $("input[name='CROCount']:checked").val();

        if (val == "TWO") {
            $("#divCRO2").slideDown();
        }

    };

    //Shows customer type div based on selection
    ShowCustomerTypeDiv();

    ShowCenter();

    ShowDemo();

    ShowBatch();

    InitRelationTable();

    ShowCRODiv();

    //Get District on State Change
    $(document).on("change", "#ddlState", GetDistrict);

    //Get Course on Qualification Change
    $(document).on("change", "#ddlQlfnType", GetCourse);

    //Get Stream on Course Change
    $(document).on("change", "#ddlQlfnMain", GetStream);

    //Get Stream on Course Change
    $(document).on("change", "#ddlQlfnSub", ValidateCustomerType);

    //Showing div based on customer type
    $(document).on("change", "#ddlCustomerType", showDiv);

    //On change of preivous experience checkbox
    $(document).on("change", "#chkbxPrevExp", ShowCenter)

    //On change of IsDemoGiven checkbox
    $(document).on("change", "#chkbxDemo", ShowDemo)

    //On change of join status checkbox
    $(document).on("change", "#chkbxJoinStatus", ShowBatch)

    //Add New row to datatabe
    $(document).on("click", "#btnAddRow", function (e) {

        if (ValidateReference()) {
            AddDataTableRow();
        }

        return false;

    });

    //RowDelete
    $(document).on("click", ".rowDelete", function () {
        //removing the current row
        $(this).parent().parent().parent().remove();
        var slno = 0;
        $("#tblStudentRelation tbody tr").each(function (i, el) {

            //Resetting serialno
            slno++;
            var $tdSlno = $(this).find('.slno');
            $tdSlno.html(slno);

            //Resetting names of all texboxes and span
            var $tdRefName = $(this).find('.refName');
            var $tdRefRelation = $(this).find('.refRelation');
            var $tdRefEmailId = $(this).find('.refEmailId');
            var $tdRefMobile = $(this).find('.refMobile');

            var $spanEmailId = $(this).find('.spanEmailId');

            $tdRefName.removeAttr('name');
            $tdRefRelation.removeAttr('name');
            $tdRefEmailId.removeAttr('name');
            $tdRefMobile.removeAttr('name');

            $spanEmailId.removeAttr('data-valmsg-for');

            $tdRefName.attr('name', 'StudentRelation[' + i + '].Name');
            $tdRefRelation.attr('name', 'StudentRelation[' + i + '].Relation');
            $tdRefEmailId.attr('name', 'StudentRelation[' + i + '].EmailId');
            $tdRefMobile.attr('name', 'StudentRelation[' + i + '].MobileNo');

            $spanEmailId.attr('data-valmsg-for', 'StudentRelation[' + i + '].EmailId');

        });

        return false;

    });

    //Percentage checking on textbox CRO1 keyup function   
    $('#txtCRO1Percentage').keyup(function () {
        //Gets the value of checked checkbox
        var val = $("input[name='CROCount']:checked").val();
        //if value is two
        if (val == "TWO") {
            var cro1Percentage = $(this).val();
            if (cro1Percentage != "") {
                cro1Percentage = parseInt(cro1Percentage);
                if (cro1Percentage >= 100 || cro1Percentage == 0) {
                    bootbox.alert("You can enter only numbers between 1 && 99");
                    $("#txtCRO1Percentage").val(50);
                    $("#txtCRO2Percentage").val(50);
                    return false;
                }
                else {
                    var cro2Percentage = 100 - cro1Percentage
                    $("#txtCRO2Percentage").val(cro2Percentage);
                }
            }
            else {
                $("#txtCRO2Percentage").val("");
            }
        }
    });

    //Percentage checking on textbox CRO2 keyup function   
    $('#txtCRO2Percentage').keyup(function () {
        //Gets the value of checked radiobutton
        var val = $("input[name='CROCount']:checked").val();

        //if value is two
        if (val == "TWO") {
            var cro2Percentage = $(this).val();
            if (cro2Percentage != "") {
                var cro2Percentage = parseInt($(this).val());
                if (cro2Percentage >= 100 || cro2Percentage == 0) {
                    bootbox.alert("You can enter only numbers between 1 && 99");
                    $("#txtCRO1Percentage").val(50);
                    $("#txtCRO2Percentage").val(50);
                    return false;
                }
                else {
                    var cro1Percentage = 100 - cro2Percentage
                    $("#txtCRO1Percentage").val(cro1Percentage);
                }
            }
            else {
                $("#txtCRO1Percentage").val("");
            }


        }
    });


    //hide/show CRO div section based on radiobutton change
    $(document).on('ifChecked', '.single', function (event) {
        var croCount = $(this).val();
        //resetting dropdownlist to make sure that CRO1 and CRO2 is not same
        $("#ddlCRO2").select2("val", "");
        if (croCount == "ONE") {
            $("#divCRO2").slideUp();
            $("#txtCRO1Percentage").val(100);
            $("#txtCRO2Percentage").val(0);
            $('#txtCRO1Percentage').prop('readonly', true);
        }
        else {
            $("#divCRO2").slideDown();
            $("#txtCRO1Percentage").val(50);
            $("#txtCRO2Percentage").val(50);
            $('#txtCRO1Percentage').prop('readonly', false);
        }

    });
})