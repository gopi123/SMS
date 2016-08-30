
$(function () {
    var form = $("#frmAdd");
    form.validate({
        errorPlacement: function errorPlacement(error, element) { element.before(error); }
    });
    form.find("#CourseUpgradationStepz").steps({
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

            //if the tab is payment details
            if (newIndex == 2) {
                InitPaymentTable();
            }

            return form.valid();
        },
        onFinishing: function (event, currentIndex) {

            $(form).data('validator', null);
            $.validator.unobtrusive.parse($('form'));
            form.validate().settings.ignore = ":disabled,:hidden";

            if (form.valid()) {
                return ResetFeeDetails();
            }
        },
        onFinished: function (event, currentIndex) {
            $(this).steps("previous");
            $(this).steps("previous");
            $(this).steps("previous");
            ajaxInsert();

        }
    });

    //text-input to uppercase
    $(".form-control").addClass('capitalise');

    $("#txtDiscount").tooltip({ placement: 'left' });

    //iCheck for checkbox and radio inputs
    $('input[type="radio"].minimal').iCheck({
        radioClass: 'iradio_square-blue',
        increaseArea: '20%'
    });

    //iCheck for checkbox and radio inputs
    $('input[type="checkbox"].minimal').iCheck({
        checkboxClass: 'icheckbox_square-blue',
        increaseArea: '20%'
    });

    //toastr options
    toastr.options = {
        "closeButton": true,
        "positionClass": "toast-bottom-right",
        "progressBar": true
    }

    $("#ddlCRO1").select2({
        placeholder: "Select Employee",
        allowClear: true
    });


    $("#ddlCRO2").select2({
        placeholder: "Select Employee",
        allowClear: true
    });

    $("#ddlInstallmentNo").select2({
        placeholder: "Select no of Installment",
        allowClear: true
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

    //////////////////Personal Details/////////////////////////////////////////

    //Remove blank spaces from select options
    $('select option').filter(function () {
        return !this.value || $.trim(this.value).length == 0 || $.trim(this.text).length == 0;
    }).remove();

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


    //Show Modal on MobileChange click
    var showModal = function () {
        $("#mdlNewMobNo").val("");
        $("#mdlPinNo").val("");
        $("#spanValidationMobError").html("");
        $("#spanValidationError").html("");
        $("#divMobileNo").show();
        $("#btnContinue").show();
        $("#divPinNo").hide();
        $("#btnOK").hide();
        $("#divModalPinVerification").modal({
            backdrop: 'static'
        });
    }

    //PinNo sending to students mobile no on continue button click
    var pinNoSending = function () {
        var href = $(this).data("href");
        var mobNo = $("#mdlNewMobNo").val();
        var currMobNo = $("#hFieldStudentMobile").val();
        if (mobNo != "") {
            if (mobNo.length == 10) {
                if (mobNo != currMobNo) {
                    $.ajax({
                        type: "GET",
                        url: href,
                        data: { studMobileNo: mobNo },
                        datatype: "json",
                        success: function (response) {
                            if (response.Status == "success") {
                                $("#divMobileNo").slideUp();
                                $("#btnContinue").slideUp();
                                $("#divPinNo").slideDown();
                                $("#btnOK").slideDown();
                                $("#mdlReturnPinNo").val(response.Data)
                            }
                            else if (response.Status == "exist") {
                                $("#divMdlErrorMobNo").show();
                                $("#spanValidationMobError").html("MobileNo already exists for " + response.Data);
                            }
                            else {
                                toastr.error("Error:Something gone wrong")
                            }
                        },
                        error: function (data) {
                            toastr.error("Exception:Something gone wrong");
                        }
                    });
                }
                else {
                    $("#divMdlErrorMobNo").show();
                    $("#spanValidationMobError").html("Please enter different MobileNo");
                }


            }
            else {
                $("#divMdlErrorMobNo").show();
                $("#spanValidationMobError").html("Please enter valid MobileNo");
            }
        }
        else {
            $("#divMdlErrorMobNo").show();
            $("#spanValidationMobError").html("Please enter MobileNo");
        }



    }

    //Verifying pinno with the return pinno
    var pinNoVerification = function () {
        var enteredPinNo = $("#mdlPinNo").val();
        var currentPinNo = $("#mdlReturnPinNo").val();

        if (enteredPinNo != "") {
            if (enteredPinNo == currentPinNo) {
                var mobNo = $("#mdlNewMobNo").val();
                $("#hFieldStudentMobile").val(mobNo);
                $("#txtMobileNo").val(mobNo);
                $("#divModalPinVerification").modal('hide');
            }
            else {
                $("#divMdlErrorPinNo").show();
                $("#spanValidationError").html("Invalid PinNo");
            }

        }
        else {
            $("#divMdlErrorPinNo").show();
            $("#spanValidationError").html("Please enter PinNo");
        }
    }
   
    
    //Show Modal on MobileChange click
    $(document).on("click", "#validateMobileNo", showModal)

    $(document).on("click", "#btnContinue", pinNoSending)

    $(document).on("click", "#btnOK", pinNoVerification)

    //Changing emailId if user has changed the email
    $("#txtEmailId").change(function () {
        var currEmail = $("#hFieldEmailId").val();
        var newEmail = $("#txtEmailId").val();

        if (currEmail != newEmail) {
            $("#hFieldEmailId").val(newEmail);
            
        }
    });


    //////////////////Course Details/////////////////////////////////////////

    var showInstallmentDiv = function () {
        var instType = $(this).val();
        $("#ddlMultiCourse").select2("val", "");
        $("#txtCourseTitle").val("");
        $("#txtSoftwareUsed").val("");
        $("#txtDuration").val("0");
        $("#txtDiscount").val("0");
        $("#txtFee").val("0");
        $("#txtCourseFee").val("0");
        $("#txtSTAmt").val("0");
        $("#txtTotalFee").val("0");
        $("#txtTotalAmt").val("0");
        $("#txtMultiCourseId").val("");
        $("#txtSingleCourseIds").val("");
        $("#txtMultiCourseCode").val("");
        if (instType == "SINGLE") {
            $("#divInstallmentNo").slideUp();
        }
        else {
            $("#divInstallmentNo").slideDown();
        }
    }

    var url = $("#divCourseCode").data("url");
    $("#ddlMultiCourse").select2({
        placeholder: "Search Course",
        minimumInputLength: 1,
        ajax: { // instead of writing the function to execute the request we use Select2's convenient helper
            url: url,
            type: "POST",
            dataType: 'json',
            delay: 250,
            data: function (params) {
                return {
                    term: params.term, // search term
                    value: GetCourseIds()
                };
            },
            processResults: function (data, params) {

                return {
                    results: data

                };
            }

        }
    });

    var GetCourseIds = function () {
        var prevCourseId = $("#hFieldPrevCourseId").val();
        var currCourseId = $("#txtMultiCourseId").val();
        var prev_curr_CourseId;
        if (currCourseId != "") {
            prev_curr_CourseId = prevCourseId + "," + currCourseId;
        }
        else {
            prev_curr_CourseId = prevCourseId;
        }
        return prev_curr_CourseId;

    }

    //ddlMulticourse select/unselect is called here
    $("#ddlMultiCourse").on("select2:select select2:unselect", function (e) {

        $("#txtDiscount").val("0");
        var courseId = $(this).val();
        //For GetCourseList calculation
        $("#txtMultiCourseId").val(courseId);

        //Gets the last selected item
        var lastSelectedCourseId = e.params.data.id;

        var feeMode = $("input[name='InstallmentType']:checked").val();
        if (e.type == "select2:select") {

            GetCourseDetails(lastSelectedCourseId, feeMode, "add");
        }
        else {

            GetCourseDetails(lastSelectedCourseId, feeMode, "subtract");

        }


    });

    var GetCourseDetails = function (multiCourseId, feeMode, type) {

        //var spinner = $(this).parent('div').find('.spinner');        
        var href = $("#divCourseCode").data("details-url");
        if (multiCourseId != "" && feeMode != "") {
            //$(spinner).toggle(true);

            $.ajax({
                type: "GET",
                url: href,
                data: { multiCourseId: multiCourseId, feeMode: feeMode },
                datatype: "json",
                success: function (data) {
                    //$(spinner).toggle(false);

                    //current coursetitle value
                    var currentCourseTitle = $("#txtCourseTitle").val();
                    //current currentSoftwareUsed value
                    var currentSoftwareUsed = $("#txtSoftwareUsed").val();
                    //current currentDuration value
                    var currentDuration = parseInt($("#txtDuration").val());
                    //current currentFee value
                    var currentFee = parseInt($("#txtCourseFee").val());
                    //current ST value
                    var currentST = parseInt($("#txtST").val());
                    //curret TotalAmount
                    var currentTotalAmt = parseInt($("#txtTotalAmt").val());
                    //current multicourse code
                    var currentMultiCourseCode = $("#txtMultiCourseCode").val();
                    //current singlecourse id
                    var currentSingleCourseId = $("#txtSingleCourseIds").val();


                    //new Course Title value
                    var newCourseTitle = data[0].CourseTitle;
                    //new Software Used value
                    var newSoftwareUsed = data[0].SoftwareUsed;
                    //new new Duration value
                    var newDuration = parseInt(data[0].Duration);
                    //new Fee value
                    var newFee = parseInt(data[0].Fee);
                    //new multicourse code
                    var newMultiCourseCode = data[0].CourseCode;
                    //new singlecourse id
                    var newSingleCourseId = data[0].CourseIds;


                    //For adding a course
                    if (type == "add") {

                        //for first coursetitle
                        if (currentCourseTitle == "") {
                            $("#txtCourseTitle").val(newCourseTitle);
                        }
                            //append commas to existing coursetitle 
                        else {
                            currentCourseTitle = currentCourseTitle + "," + newCourseTitle;
                            $("#txtCourseTitle").val(currentCourseTitle);
                        }
                        //for first softwareused
                        if (currentSoftwareUsed == "") {
                            $("#txtSoftwareUsed").val(newSoftwareUsed);
                        }
                            //append commas to existing softwareused 
                        else {
                            currentSoftwareUsed = currentSoftwareUsed + "," + newSoftwareUsed;
                            $("#txtSoftwareUsed").val(currentSoftwareUsed);
                        }
                        //for first single courseid
                        if (currentSingleCourseId == "") {
                            $("#txtSingleCourseIds").val(newSingleCourseId);
                        }
                            //append commas to existing single courseid 
                        else {
                            currentSingleCourseId = currentSingleCourseId + "," + newSingleCourseId;
                            $("#txtSingleCourseIds").val(currentSingleCourseId);
                        }

                        //for first current duration
                        if (currentDuration == 0) {
                            currentDuration = newDuration;
                            $("#txtDuration").val(currentDuration);
                        }
                            //adding newduration to existing duration 
                        else {
                            currentDuration = currentDuration + newDuration;
                            $("#txtDuration").val(currentDuration);
                        }

                        //for first time current fee
                        if (currentFee == 0) {
                            currentFee = newFee;
                            $("#txtCourseFee").val(currentFee);
                        }
                            //adding newfee to current fee     
                        else {
                            currentFee = currentFee + newFee;
                            //For calculation purposes
                            $("#txtCourseFee").val(currentFee);
                        }

                        if (currentMultiCourseCode == "") {
                            $("#txtMultiCourseCode").val(newMultiCourseCode);
                        }

                        else {
                            currentMultiCourseCode = currentMultiCourseCode + "," + newMultiCourseCode;
                            $("#txtMultiCourseCode").val(currentMultiCourseCode);
                        }



                        GetFeeDetails();


                    }
                        //For removing a course
                    else {

                        //removing unselected coursetitle from textbox
                        currentCourseTitle = currentCourseTitle.replace(newCourseTitle, "");
                        //removing unselected softwareused from textbox
                        currentSoftwareUsed = currentSoftwareUsed.replace(newSoftwareUsed, "");
                        //removing unselected course code
                        currentMultiCourseCode = currentMultiCourseCode.replace(newMultiCourseCode, "");
                        //removing unselected course ids
                        currentSingleCourseId = currentSingleCourseId.replace(newSingleCourseId, "");
                        //subtracting unselected couse duration
                        currentDuration = currentDuration - newDuration;
                        //subtracting unselected coursefee
                        currentFee = currentFee - newFee;


                        //removing commas from the end and start from coursetitle textbox
                        currentCourseTitle = currentCourseTitle.replace(/^,|,$/g, '');
                        //removing commas from the end and start from softwareused textbox
                        currentSoftwareUsed = currentSoftwareUsed.replace(/^,|,$/g, '');
                        //removing commas from the end and start
                        currentMultiCourseCode = currentMultiCourseCode.replace(/^,|,$/g, '');
                        //removing commas from the end and start
                        currentSingleCourseId = currentSingleCourseId.replace(/^,|,$/g, '');




                        $("#txtCourseTitle").val(currentCourseTitle);
                        $("#txtSoftwareUsed").val(currentSoftwareUsed);
                        $("#txtDuration").val(currentDuration);
                        $("#txtCourseFee").val(currentFee);
                        $("#txtMultiCourseCode").val(currentMultiCourseCode);
                        $("#txtSingleCourseIds").val(currentSingleCourseId);


                        GetFeeDetails();


                    }
                },
                error: function (err) {
                    //$(spinner).toggle(false);
                    toastr.error("Error:" + this.message)
                }
            });
        }
        else {
            //$ddlDistrict.html('');//clearing the dpdwn html
            //$ddlDistrict.select2("val", "");//resetting dpdwn for clearing the selected option
        }

    };

    //calculation of fees on course selection/unselection/discount/roundup is performed here
    var GetFeeDetails = function () {

        //Gets the current discount percentage
        var currDiscount = parseInt($("#txtDiscount").val());
        //Gets the current fee
        var courseFee = parseInt($("#txtCourseFee").val());
        //Gets the current ST percentage
        var currST = Number($("#txtST").val());

        //TotalAmount Calculation
        var stAmt = Math.round((courseFee) * (currST / 100));
        var currTotalAmt = courseFee + stAmt;


        //Gets the new discount amount
        var discountAmt = Math.round((currTotalAmt) * (currDiscount / 100));
        //Gets the new Total Amt
        var newTotalAmt = parseInt(currTotalAmt - discountAmt);

        //Applying RoundUp or RoundOff
        //Gets current roundup value
        var roundUpValue = $("#ddlRoundUpList").val();
        //roundup 10
        if (roundUpValue == 1) {
            newTotalAmt = Math.ceil(newTotalAmt / 10) * 10;
        }
            //roundoff
        else {
            newTotalAmt = Math.round(newTotalAmt / 10) * 10
        }

        //calculating coursefee from new total amt
        courseFee = Math.round((newTotalAmt) / ((100 + currST) / 100));
        //Gets the new st amt
        var newSTAmt = parseInt(newTotalAmt - courseFee);



        $("#txtFee").val(courseFee);
        $("#txtSTAmt").val(newSTAmt);
        $("#txtTotalFee").val(newTotalAmt);
        $("#txtTotalAmt").val(currTotalAmt);

        return false;
    };

    //send sms to concerned centermanager for discount amt greater than 60%
    var smsVerificationOnDiscountChange = function () {
        var form = $("#frmAdd");
        if (form.valid()) {
            var currDiscPercentage = Number($("#txtDiscount").val());
            var defaultDiscountPercentage = Number($("#txtDefaultDiscountPercentage").val());
            if (currDiscPercentage > defaultDiscountPercentage) {
                var box = bootbox.confirm("DiscountPercentage is greater than the allowed discount percentage of <b>" + defaultDiscountPercentage + "</b>.Requires <b>Pin Verification</b> from the concerned <b>Centre Manager</b>.Click <b>OK</b> to proceed.", function (result) {
                    if (result) {
                        box.modal('hide');
                        $.blockUI({ message: null });

                        var href = $("#divModalDiscountVerification").data("url");
                        var studName = $("#txtStudentName").val();
                        var discPercentage = $("#txtDiscount").val();
                        var centreCode = $("#hfieldCentreCode").val();

                        $.ajax({
                            type: "GET",
                            url: href,
                            data: { studentName: studName, discountPercentage: discPercentage, centreCode: centreCode },
                            datatype: "json",
                            success: function (data) {
                                if (data == "employee_error") {
                                    $("#txtDiscount").val(0);
                                    GetFeeDetails();
                                    $.unblockUI();
                                    bootbox.alert("Employee not yet assigned to the <b>" + centreCode + "</b>")
                                }
                                else if (data == "error") {
                                    $("#txtDiscount").val(0);
                                    GetFeeDetails();
                                    $.unblockUI();
                                    bootbox.alert("Error some thing happened")
                                }
                                else {
                                    $("#mdlPinNo_Discount").val('');
                                    $("#divMdlErrorPinNo_Discount").hide();
                                    $("#mdlReturnPinNo_Discount").val(data);
                                    $.unblockUI();
                                    $("#divModalDiscountVerification").modal({
                                        backdrop: 'static'
                                    });
                                }
                            },
                            error: function (err) {
                                $("#txtDiscount").val(0);
                                GetFeeDetails();
                                toastr.error("Error:" + this.message);
                            }
                        });
                    }
                    else {
                        $("#txtDiscount").val(0);
                        GetFeeDetails();
                    }
                });
            }
        }
    }

    //on clicking continue on discountverification modal
    $(document).on("click", "#btnContinue_Discount", function () {
        var returnPinNo = $("#mdlReturnPinNo_Discount").val();
        var currentPinNo = $("#mdlPinNo_Discount").val();

        if (returnPinNo == currentPinNo) {
            var currDiscountPercentage = $("#txtDiscount").val();
            $("#txtDefaultDiscountPercentage").val(currDiscountPercentage);
            $("#divModalDiscountVerification").modal('hide');
            GetFeeDetails();
        }
        else {
            $("#divMdlErrorPinNo_Discount").show();
        }

    })

    //On clicking close of discountverification modal
    $(document).on("click", "#btnMdlClose_Discount", function () {
        $("#txtDiscount").val(0);
        GetFeeDetails();

    })

    //Discount calculation is performed here
    var GetDiscount = function () {
        //smsVerificationOnDiscountChange();
        GetFeeDetails();

    }

    var GetRoundUpDetails = function () {
        GetFeeDetails();
    }

    //hide/show installment section based on installmentradiobutton change
    $(document).on('ifChecked', '.installment', showInstallmentDiv)

    $(document).on("change", "#txtDiscount", GetDiscount);

    //ddlRoundupList change function
    $(document).on("change", "#ddlRoundUpList", GetRoundUpDetails);

    $("#txtDiscount").change(function () {
        smsVerificationOnDiscountChange();
    });

    /////////////////////Payment Details ///////////////////////////////////

    //Payment table creation is done here
    var InitPaymentTable = function () {
        var count;
        var feeMode = $("input[name='InstallmentType']:checked").val();
        if (feeMode == "SINGLE") {
            count = 2;
        }
        else {
            count = parseInt($("#ddlInstallmentNo").val());
        }
        //For firsttime addition of datatable
        var $tbody = $("#tblPaymentDetails tbody");
        $($tbody).html('');

        for (var i = 0; i < count; i++) {

            var slno = parseInt(i + 1);

            var $row = $('<tr/>');
            $row.append(' <td class="slno">' + slno + '</td>');

            $row.append(' <td><input name="StudentReceipt[' + i + '].Fee" type="text" class="form-control  valid courseFee" data-val="true" data-val-required="Fee required" placeholder="Fee" readonly="readonly" />'
                           + '</td>');

            $row.append(' <td><input name="StudentReceipt[' + i + '].ST" type="text" class="form-control valid stAmt" data-val="true" data-val-required="ST required" placeholder="ST" readonly="readonly"  />'
                           + ' </td>');

            $row.append(' <td><input name="StudentReceipt[' + i + '].Total" type="text" class="form-control valid totalAmt numberOnly" data-val="true" data-val-required="TotalAmount required" placeholder="Total"  />'
                            + '<span class="field-validation-valid spanEmailId" data-valmsg-for="StudentReceipt[' + i + '].Total" data-valmsg-replace="true"></span> </td>');

            $row.append(' <td><input name="StudentReceipt[' + i + '].DueDate" type="text" class="form-control valid dueDate" data-val="true" data-val-required="Duedate required" placeholder="DueDate" id="txtDueDate' + i + '" readonly="readonly" style="background:white" />'
                            + '<span class="field-validation-valid spanAltContactNumber" data-valmsg-for="StudentReceipt[' + i + '].DueDate" data-valmsg-replace="true"></span> </td>');


            $tbody.append($row);

        }

        GetCallOutMessage();
        GetDatepicker();
        InitPaymentDetails();
        GetPaymentDetailsOnTxtbxChange();
    }

    //Gets the callout message of payment details
    var GetCallOutMessage = function () {
        var totalFee = "TotalFee - &#8377; " + $("#txtFee").val();
        var totalST = "TotalST - &#8377; " + $("#txtSTAmt").val();
        var totalAmt = "TotalAmount - &#8377; " + $("#txtTotalFee").val();
        $("#titleTotalamt").html(totalFee + ' ' + ' ' + totalST + ' ' + totalAmt);
    };

    //Datepicker settings
    var GetDatepicker = function () {
        //Datepicker section
        var datepickers;
        var $tbody = $("#tblPaymentDetails tbody");
        datepickers = $tbody.find('.dueDate');
        //console.log(datepickers);

        $('.dueDate').datepicker({
            autoclose: true,
            startDate: '+0d', // set default to today's date          
        }).on('changeDate', function (e) {
            //console.log(e.date); // should return the selected date
            var index = datepickers.index($(this)); // should return '1' if you selected the datepicker in the second row            
            // loop through all the datepickers after this one
            for (var i = index + 1; i < datepickers.length; i++) {
                // set the startDate based on the date of this one
                datepickers.eq(i).datepicker('setStartDate', e.date);
            }
        });
    }

    //Payment details initialization is performed here
    var InitPaymentDetails = function () {

        //gets the rows of datatable
        var $tr = $("#tblPaymentDetails tbody tr");
        //gets the totalfee
        var totalFee = $("#txtTotalFee").val();
        var currTotal = 0;
        //gets the totalst
        var totalST = Number($("#txtST").val());
        //totalST percentage calcuation
        totalST = ((totalST + 100) / 100);

        for (var i = 0; i < 2; i++) {
            //gets the current totalamount textbox
            var $txtTotalAmt = $($tr[i]).find('.totalAmt');
            //for first row
            if (i == 0) {
                //1000 is the default value
                currTotal = 1000;
                //sets the value to the current totalamount textbox
                $($txtTotalAmt).val(currTotal);
            }
                //for second row
            else {
                //gets the remaining total amount value
                currTotal = parseInt(totalFee - 1000);
                //sets the value to the current total amount textbox
                $($txtTotalAmt).val(currTotal);
            }
            //gets the stamt textbox
            var $txtSTAmt = $($tr[i]).find('.stAmt');
            //gets the coursefee textbox
            var $txtCourseFee = $($tr[i]).find('.courseFee');

            //coursefee calculation
            var currCourseFee = Math.round(currTotal / totalST);
            //currentst calculation
            var currSTAmt = currTotal - currCourseFee;

            //set the value of coursefee to current coursefee textbox
            $($txtCourseFee).val(currCourseFee);
            //sets the value of st to current st textbox
            $($txtSTAmt).val(currSTAmt);

        }
    }

    //calculation of payment details of textboxchange
    var GetPaymentDetailsOnTxtbxChange = function () {

        var totalAmount;
        var totalCourseFee;
        var totalSTAmt;

        var totalCourseAmount = parseInt($("#txtTotalFee").val());
        var totalSTPercent = Number($("#txtST").val());
        totalSTPercent = (100 + totalSTPercent) / 100;

        var $tbody = $("#tblPaymentDetails tbody");
        totalAmount = $tbody.find('.totalAmt');
        totalCourseFee = $tbody.find('.courseFee');
        totalSTAmt = $tbody.find('.stAmt');

        $('.totalAmt').change(function () {
            //Minimum amount set is 100
            if ($(this).val() < 100) {
                bootbox.alert("Minimum amount should be 100");
                return false;
            }
            //gets the index 
            var index = totalAmount.index($(this));
            console.log(index);
            //getting current course fee
            var currCourseFee = $(totalCourseFee.eq(index)).val();
            //getting current ST Amount
            var currSTAmt = $(totalSTAmt.eq(index)).val();
            //getting current Total amount
            var currTotalAmt = $(totalAmount.eq(index)).val();

            //calculating coursefee
            currCourseFee = Math.round(currTotalAmt / totalSTPercent);
            //calculating st amount
            currSTAmt = parseInt(currTotalAmt - currCourseFee);

            //setting the value of coursefee
            $(totalCourseFee.eq(index)).val(currCourseFee);
            //setting the value of stamt
            $(totalSTAmt.eq(index)).val(currSTAmt);


            //adding the current totalamounts
            currTotalAmt = 0;
            for (var i = 0; i <= index; i++) {
                currTotalAmt = currTotalAmt + parseInt($(totalAmount.eq(i)).val());
            }

            //calculates the nextrow totalamount
            var nxtTotalAmt = parseInt(totalCourseAmount - currTotalAmt);
            //calculates the nextrow coursefee
            var nxtCourseFee = Math.round(nxtTotalAmt / totalSTPercent);
            //calculates the nextrow stamount
            var nxtSTAmt = parseInt(nxtTotalAmt - nxtCourseFee);

            //setting the value of nextrow coursefee
            $(totalCourseFee.eq(index + 1)).val(nxtCourseFee);
            //setting the value of nextrow stamount
            $(totalSTAmt.eq(index + 1)).val(nxtSTAmt);
            //setting the value of nextrow totalamount
            $(totalAmount.eq(index + 1)).val(nxtTotalAmt);

            //clearing all the remaining textboxes
            for (var i = index + 2; i < totalAmount.length; i++) {
                $(totalCourseFee.eq(i)).val('');
                $(totalSTAmt.eq(i)).val('');
                (totalAmount.eq(i)).val('');
            }
        });

    };



    ///////////////////////Saving Details ////////////////////////////////////

    //resetting coursefee and servicetax according to the sum of table details
    var ResetFeeDetails = function () {
        var validate = false;
        var $tr = $("#tblPaymentDetails tbody tr");
        var totalAmt = 0;
        var totalCourseFee = 0;
        var totalST = 0;
        var overAllTotalAmt = $("#txtTotalFee").val();

        //adding corresponding feedetails
        for (var i = 0; i < $tr.length; i++) {
            var currTotalAmt = parseInt($($tr[i]).find('.totalAmt').val());
            totalAmt = totalAmt + parseInt($($tr[i]).find('.totalAmt').val());
            totalCourseFee = totalCourseFee + parseInt($($tr[i]).find('.courseFee').val());
            totalST = totalST + parseInt($($tr[i]).find('.stAmt').val());

            //if any one of the total amount is less than 100
            if (currTotalAmt < 100) {
                validate = false;
                bootbox.alert("Minimum amount should be 100");
                $($tr[i]).find('.totalAmt').focus();
                return validate;
            }
        }

        if (overAllTotalAmt == totalAmt) {
            validate = true;
            $("#txtFee").val(totalCourseFee);
            $("#txtSTAmt").val(totalST);
            GetCallOutMessage();

        }
        else {
            validate = false;
            bootbox.alert("TotalAmount Mismatch");
        }

        return validate;
    }

    var ajaxInsert = function () {

        var currAmount = $("#tblPaymentDetails").find('tbody tr:first').find('.totalAmt').val();
        var studentName = $("#txtStudentName").val();
        var receiptDate = new Date($("#tblPaymentDetails").find('tbody tr:first').find('.dueDate').val());
        receiptDate = receiptDate.getDate() + '/' + (receiptDate.getMonth() + 1) + '/' + receiptDate.getFullYear();

        bootbox.confirm("Do you want to proceed with the payment of <b>Rs." + currAmount + "(Including ST)</b>", function (result) {

            if (result) {

                $("#txtMdlStudentName").val(studentName);
                $("#txtMdlReceiptAmt").val(currAmount);
                $("#txtMdlReceiptDate").val(receiptDate);

                $("#divModalReceipt_CourseUpgrade").modal({
                    backdrop: 'static'
                });

                bootbox.hideAll()
                return false;

            }

        });

    };


    $(document).on("click", "#btn_CourseUpgrade_PayNow", function () {
        var form = $("#frmAdd");


        //show alert if confirm payment checkbox has not been checked
        var isPaymentConfirmed = $("#chkbxConfirm_Receipt").prop("checked");
        if (isPaymentConfirmed) {

            $("#divModalReceipt_CourseUpgrade").modal('hide');

            $("#frmAdd").ajaxSubmit({
                iframe: true,
                dataType: "json",
                beforeSubmit: function () {
                    $.blockUI({ message: '<h3><img src="../plugins/jQueryBlockUI/images/busy.gif" /> <b>Please wait... </b></h3>' });

                },
                success: function (result) {

                    if (result.Status == "success") {
                        generateReceipts(result.RegistrationId);
                    }
                    else if (result.Status == "error_sms_student") {
                        toastr.error("Error while sending registration sms student")
                    }
                    else if (result.Status == "error_email_student") {
                        toastr.error("Error while sending email to student")
                    }
                    else if (result.Status == "error_sms_official") {
                        toastr.error("Error while sending sms to official")
                    }
                    else {
                        toastr.error("Error:Something gone wrong")
                    }
                },
                error: function (xhr, textStatus, errorThrown) {
                    $.unblockUI();
                    toastr.error("Error:Something gone wrong")
                }

            });
        }
        else {
            var $div = $("#chkbxConfirm_Receipt").closest('.divConfirmation');
            $($div).append("<div class='payment-confirmation field-validation-error'>Please confirme the above </div>")
            setTimeout(function () {
                $('.payment-confirmation').fadeOut(1500);

            }, 3000);
        }



    });

    var generateReceipts = function (studRegId) {
        var href = $("#frmAdd").data("receipt-generate-url");
        $.ajax({
            type: "GET",
            url: href,
            data: { studentRegID: studRegId },
            datatype: "json",
            success: function (data) {
                $.unblockUI();
                if (data.Status == "success") {
                    toastr.success("Successfully saved the details.");
                    setTimeout(function () {
                        var url = $(form).data("redirect-url");
                        url = url.replace('param1_placeholder', data.RegistrationId);
                        window.location.href = url;
                    }, 2000);
                    var printpdf_download_url = $(form).data("printpdf-url");
                    printpdf_download_url = printpdf_download_url.replace('param2_placeholder', data.PdfName + ".pdf");
                    window.location.href = printpdf_download_url;

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

});