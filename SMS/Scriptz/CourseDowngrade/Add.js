
//CourseDowngrade ====> Add.js

$(function () {
    var form = $("#frmAdd");
    form.validate({
        errorPlacement: function errorPlacement(error, element) { element.before(error); }
    });
    form.find("#CourseDowngradeStepz").steps({
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
                if (currentIndex == 0) {
                    GetPaidAmountDetails();
                }

                //On clicking NEXT of Course Details
                if (currentIndex == 1) {
                    if (!ValidateCourse()) {
                        return false;
                    }
                }
                //if the tab is payment details
                if (newIndex == 2) {
                    if (InstallmentChangeValidation()) {
                        InitPaymentTable();
                    }
                    else {
                        return false;
                    }

                }

                //if the tab is pinverification
                if (newIndex == 3) {
                    //if (validateTableRows()) {
                    //    //GetPinNo();
                    //}
                    //else {
                    //    return false;
                    //}
                    if (ResetFeeDetails()) {
                        GetPinNo();
                    }
                    else {
                        return false;
                    }
                    
                }
                return true;
            }
            else {
                return form.valid();
            }

        },
        onFinishing: function (event, currentIndex) {

            if (form.valid()) {
                return ValidatePinDetails();

            }
        },
        onFinished: function (event, currentIndex) {
            GetManagerPinVerification();

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

    $("#ddlCourseDowngradeId").select2({
        placeholder: "Select course to downgrade",
        allowClear: true
    });

    $("#ddlMultiCourse").select2({
        placeholder: "Select CourseCode",
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



    /////////////////////Course Details ////////////////////// 

    var GetFeeModeDiv = function () {
        var feeMode = $("input[name='InstallmentType']:checked").val();
        if (feeMode == "SINGLE") {
            $("#divInstallmentNo").slideUp();
        }
        else {
            $("#divInstallmentNo").slideDown();
        }

        //Enabling/Disabling installment radiobutton    
        var count = $('input[value=True]').closest('tr').length;
        if (count >= 2) {
            $('.installment').iCheck('disable');
        }
    }


    var GetCourseFee = function () {
        var feeMode = $(this).val();
        var href = $("#divFeeMode").data("url");
        var multiCourseId = $("#txtMultiCourseId").val();


        //hide or show installment div
        if (feeMode == "SINGLE") {
            $("#divInstallmentNo").slideUp();
        }
        else {
            $("#divInstallmentNo").slideDown();
        }

        //Getting coursefee based on feemode
        $.ajax({
            type: "POST",
            url: href,
            data: { multiCourseId: multiCourseId, feeModeType: feeMode },
            datatype: "json",
            success: function (data) {

                var courseFee = Number(data);
                if (courseFee != 0) {
                    $("#txtCourseFee").val(courseFee)
                    GetFeeDetails();
                }
                else {
                    bootbox.alert("Cannot get CourseFee value");
                }
            },
            error: function (data) {
                bootbox.alert("Exception:Something gone wrong");
            }
        });

        return false;

    };

    var GetCourseCodeDetails = function () {
        var downgradeCourseId = $("#ddlCourseDowngradeId").val();
        var regId = $("#hFieldRegId").val();
        var $ddlCourseCode = $("#ddlMultiCourse");
        var href = $("#divCourseCode_Dwngrade").data("url");
        var spinner = $(this).parent('div').parent('div').find('.spinner');

        if (downgradeCourseId != "") {
            $(spinner).toggle(true);
            $.ajax({
                type: "GET",
                url: href,
                data: { downgradeCourseId: downgradeCourseId, regId: regId },
                datatype: "json",
                success: function (data) {
                    $(spinner).toggle(false);
                    $ddlCourseCode.html('');//clearing the dpdwn html
                    $ddlCourseCode.select2("val", "");//resetting dpdwn for clearing the selected option
                    $ddlCourseCode.append('<option></option>')
                    $.each(data, function () {
                        $ddlCourseCode.append($('<option></option>').val(this.Id).html(this.Name));
                    });
                },
                error: function (err) {
                    $(spinner).toggle(false);
                    $ddlCourseCode.html('');//clearing the dpdwn html
                    $ddlCourseCode.select2("val", "");//resetting dpdwn for clearing the selected option
                }
            });
        }
        else {
            $ddlCourseCode.html('');//clearing the dpdwn html
            $ddlCourseCode.select2("val", "");//resetting dpdwn for clearing the selected option
        }

    }

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

    var GetCourseDetails = function (multiCourseId, feeMode, type, regId) {

        //var spinner = $(this).parent('div').find('.spinner');        
        var href = $("#divCourseCode").data("details-url");
        if (multiCourseId != "" && feeMode != "") {
            //$(spinner).toggle(true);

            $.ajax({
                type: "GET",
                url: href,
                data: { multiCourseId: multiCourseId, feeMode: feeMode, regId: regId },
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
                    var newFee = parseInt(data[0].Fee_Updated);
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

    //validating paymentcount and installment change(user cannot change installment <= paymentcount)
    var InstallmentChangeValidation = function () {

        var validate = true;
        var paidCount = $('input[value=True]').closest('tr').length;
        var currInstallment = $("#ddlInstallmentNo").val();
        var remainingPaidCount = $('input[value=False]').closest('tr').length;
        //if installment was chosen else currInstallment will be ""
        if (currInstallment != "" && remainingPaidCount > 0) {


            if (currInstallment < paidCount) {
                validate = false;
                bootbox.alert("<b>" + paidCount + "</b> payments are completed.So installment cannot be changed to <b>" + currInstallment + "</b>")
            }
        }
        return validate;
    };

    //Gets the callout message of payment details
    var GetCallOutMessage = function () {
        var totalFee = "TotalFee - &#8377; " + $("#txtFee").val();
        var totalST = "TotalST - &#8377; " + $("#txtSTAmt").val();
        var totalAmt = "TotalAmount - &#8377; " + $("#txtTotalFee").val();
        $("#titleTotalamt").html(totalFee + ' ' + ' ' + totalST + ' ' + totalAmt);
    };

    var DegradePaymentTable = function () {
        var $paidtr;
        var $firstUnpaidtr;
        var currSumPaidTotal = 0;
        var currTotalAmount = 0;
        var currCourseFee = 0;
        var currSTAmount = 0;

        var totalAmount = Number($("#txtTotalFee").val());
        var currSTPercentage = Number($("#txtST").val());
        var currST = Number((100 + currSTPercentage) / 100);

        $paidtr = $('input[value=True]').closest('tr');
        $($paidtr).find('.totalAmt').each(function () {
            console.log(Number($(this).val()));
            currSumPaidTotal = currSumPaidTotal + Number($(this).val());
        });

        //Remaining amount calculation
        currTotalAmount = totalAmount - currSumPaidTotal;
        currCourseFee = Math.round(currTotalAmount / currST);
        currSTAmount = Number(currTotalAmount - currCourseFee);

        //Clearing details of all unpaid amount
        $('input[value=False]').closest('tr').each(function () {
            $(this).find(".totalAmt").val('');
            $(this).find(".stAmt").val('');
            $(this).find(".courseFee").val('');
            $(this).find(".stPercentage").val('');
            $(this).find(".dueDate").val('');
        });

        $firstUnpaidtr = $('input[value=False]:first').closest('tr');
        //gets the current totalamount textbox
        $($firstUnpaidtr).find('.totalAmt').val(currTotalAmount);
        //gets the stamt textbox
        $($firstUnpaidtr).find('.stAmt').val(currSTAmount);
        //gets the coursefee textbox
        $($firstUnpaidtr).find('.courseFee').val(currCourseFee);
        //gets the st percentage textbox
        $($firstUnpaidtr).find('.stPercentage').val(currSTPercentage);

    }

    var AddRows = function (start, end) {

        var $tbody = $("#tblPaymentDetails tbody");
        for (var i = start; i < end; i++) {
            var slno = parseInt(i + 1);
            var $row = $('<tr/>');
            $row.append('<input class="status" id="txtStatus_0"  name="StudentReceipt[' + i + '].Status" type="hidden" value="False">')

            $row.append(' <td class="slno">' + slno + '</td>');

            $row.append(' <td><input name="StudentReceipt[' + i + '].ReceiptNo" type="text" class="form-control receiptno" placeholder="RECEIPTNO" readonly="readonly" />'
                         + '</td>');

            $row.append(' <td><input name="StudentReceipt[' + i + '].Fee" type="text" class="form-control courseFee" placeholder="FEE" readonly="readonly" />'
                           + '</td>');

            $row.append(' <td><input name="StudentReceipt[' + i + '].STPercentage" type="text" class="form-control stPercentage" placeholder="ST PERCENTAGE" readonly="readonly" id="txtSTPercentage_"' + i + ' />'
                          + '</td>');

            $row.append(' <td><input name="StudentReceipt[' + i + '].ST" type="text" class="form-control stAmt" placeholder="ST" readonly="readonly"  />'
                           + '</td>');

            $row.append(' <td><input name="StudentReceipt[' + i + '].Total" type="text" class="form-control valid totalAmt numberOnly" data-val="true" data-val-required="Enter Total" placeholder="TOTAL"  />'
                            + '<span class="field-validation-valid " data-valmsg-for="StudentReceipt[' + i + '].Total" data-valmsg-replace="true"></span> </td>');


            $row.append(' <td><input name="StudentReceipt[' + i + '].DueDate" type="text" class="form-control valid dueDate date" data-val="true" data-val-required="Enter DueDate" placeholder="DUEDATE" id="txtDueDate' + i + '" readonly="readonly" style="background:white" />'
                            + '<span class="field-validation-valid " data-valmsg-for="StudentReceipt[' + i + '].DueDate" data-valmsg-replace="true"></span> </td>');

            $tbody.append($row);
        }

    }


    var GetDataTable = function () {
        var $tr;
        var currSumTotal = 0;
        var currSumPaidTotal = 0;
        var currTotalAmount = 0;
        var currCourseFee = 0;
        var currSTAmount = 0;
        var $paidtr;
        var $firstUnpaidtr;

        var currSelectedRowLength = 0;
        var gridRowLength = $("#tblPaymentDetails tbody tr").length;
        var feeMode = $("input[name='InstallmentType']:checked").val();
        var totalAmount = Number($("#txtTotalFee").val());
        var currSTPercentage = Number($("#txtST").val());
        var currST = Number((100 + currSTPercentage) / 100);

        if (feeMode == "SINGLE") {
            currSelectedRowLength = 2
        }
        else {
            currSelectedRowLength = $("#ddlInstallmentNo").val();
        }


        //upgradation
        if (currSelectedRowLength > gridRowLength) {

            $tr = $("#tblPaymentDetails tbody tr");

            //Adding FeeDetails
            $($tr).find('.totalAmt').each(function () {
                currSumTotal = currSumTotal + Number($(this).val());
            });

            //Single to installment
            if (currSumTotal != totalAmount) {
                AddRows(gridRowLength, currSelectedRowLength)

                //Calculating the sum of  paidRows
                $paidtr = $('input[value=True]:first').closest('tr');
                $($paidtr).find('.totalAmt').each(function () {
                    currSumPaidTotal = currSumPaidTotal + Number($(this).val());
                });

                //Remaining amount calculation
                currTotalAmount = totalAmount - currSumPaidTotal;
                currCourseFee = Math.round(currTotalAmount / currST);
                currSTAmount = Number(currTotalAmount - currCourseFee);

                //Clearing details of all unpaid amount
                $('input[value=False]').closest('tr').each(function () {
                    $(this).find(".totalAmt").val('');
                    $(this).find(".stAmt").val('');
                    $(this).find(".courseFee").val('');
                    $(this).find(".stPercentage").val('');
                    $(this).find(".dueDate").val('');
                });

                $firstUnpaidtr = $('input[value=False]:first').closest('tr');
                //gets the current totalamount textbox
                $($firstUnpaidtr).find('.totalAmt').val(currTotalAmount);
                //gets the stamt textbox
                $($firstUnpaidtr).find('.stAmt').val(currSTAmount);
                //gets the coursefee textbox
                $($firstUnpaidtr).find('.courseFee').val(currCourseFee);
                //gets the st percentage textbox
                $($firstUnpaidtr).find('.stPercentage').val(currSTPercentage);


            }
                //Installment upgradation
            else {
                AddRows(gridRowLength, currSelectedRowLength);

            }

        }
        //degradation
        if (currSelectedRowLength < gridRowLength) {

            //removing rows
            $("#tblPaymentDetails tbody tr:gt(" + (currSelectedRowLength - 1) + ")").remove();

            $tr = $("#tblPaymentDetails tbody tr");

            //adding all rows except the first unpaid row
            $($tr).not($('input[value=False]:first').closest('tr')).each(function () {
                currSumPaidTotal = currSumPaidTotal + Number($(this).find('.totalAmt').val());
            });

            //Remaining amount calculation
            currTotalAmount = totalAmount - currSumPaidTotal;
            currCourseFee = Math.round(currTotalAmount / currST);
            currSTAmount = Number(currTotalAmount - currCourseFee);


            $firstUnpaidtr = $('input[value=False]:first').closest('tr');
            //gets the current totalamount textbox
            $($firstUnpaidtr).find('.totalAmt').val(currTotalAmount);
            //gets the stamt textbox
            $($firstUnpaidtr).find('.stAmt').val(currSTAmount);
            //gets the coursefee textbox
            $($firstUnpaidtr).find('.courseFee').val(currCourseFee);
            //gets the st percentage textbox
            $($firstUnpaidtr).find('.stPercentage').val(currSTPercentage);

        }
    }

    //calculation of payment details of textboxchange
    var GetPaymentDetailsOnTxtbxChange = function () {

        var totalAmount;
        var totalCourseFee;
        var totalSTAmt;

        var totalFee = parseInt($("#txtTotalFee").val());
        var totalSTPercent = Number($("#txtST").val());
        totalSTPercent = (100 + totalSTPercent) / 100;

        var $tbody = $("#tblPaymentDetails tbody");
        totalAmount = $tbody.find('.totalAmt');
        totalCourseFee = $tbody.find('.courseFee');
        totalSTAmt = $tbody.find('.stAmt');
        var stPercentage = $tbody.find('.stPercentage');
        var currSTPercent = Number($("#txtST").val());

        $('.totalAmt').change(function () {

            var totalRowLength = $("#tblPaymentDetails tbody tr").length;
            //gets the index 
            var index = totalAmount.index($(this));

            //Except for the last row display the message
            if (index != totalRowLength - 1) {
                //Minimum amount set is 100
                if ($(this).val() < 100) {
                    bootbox.alert("Minimum amount should be 100");
                    return false;
                }
            }
           
           
            //console.log(index);
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

            ///////////Next Row Calculation////////////
            var nxtRowId = index + 1;

            var sumAmount = GetTotalAmtExceptCurrentRow(nxtRowId);
            //calculates the nextrow totalamount
            var nxtTotalAmt = totalFee - sumAmount;
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
            //setting the ST Percentage
            $(stPercentage.eq(index + 1)).val(currSTPercent);

        });

    };

    var GetTotalAmtExceptCurrentRow = function (currRow) {

        var rowLength = $("#tblPaymentDetails tbody tr").length;
        var $tr = $("#tblPaymentDetails tbody tr");
        var currAmount = 0;
        for (var i = 0; i < rowLength; i++) {

            if (i != currRow) {
                //gets the current totalamount textbox
                var $txtTotalAmt = $($tr[i]).find('.totalAmt');
                currAmount = currAmount + Number($($txtTotalAmt).val());
            }
        }
        return currAmount;
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


    //Payment table creation is done here
    var InitPaymentTable = function () {

        GetCallOutMessage();
        DegradePaymentTable();
        GetDataTable();
        GetPaymentDetailsOnTxtbxChange();
        GetDatepicker();
    }

    //Below function makes sure that user has not selected more courses
    //other than the students course   
    var ValidateCourse = function () {

        var Validate = true;
        var joinedCourseId = $("#hFieldCourseIds").val().split(',');        

        //Selected course to downgrade
        var dwngradedCourseId = $("#ddlCourseDowngradeId").val();

        //Getting the index of downgraded course id for removing from 
        //joined course list
        var index = joinedCourseId.indexOf(dwngradedCourseId)
        //removing downgradedcourseid from joinedcourseid
        if (index > -1) {
            joinedCourseId.splice(index, 1);
        }

        var newCourseName = $("#txtSoftwareUsed").val().split(',');
        var newCourseId = $("#txtSingleCourseIds").val().split(',');
        //Removes empty strings from the array
        newCourseId = newCourseId.filter(function (v) { return v !== '' });


        //Checks if same course has been added twice
        var duplicateCourseId = newCourseId.duplicate();
        if (duplicateCourseId.length > 0) {
            var duplicateCourseIdIndex = newCourseId.indexOf(duplicateCourseId[0]);
            var duplicateCourseName = newCourseName[duplicateCourseIdIndex];
            bootbox.alert("Cannot add <b>" + duplicateCourseName + " </b> twice.");
            Validate = false;
            return Validate;
        }

        //Checks if any extra course has been added
        var notJoinedCourseId = joinedCourseId.diff(newCourseId);

        if (notJoinedCourseId.length > 0) {
            var notJoinedCourseIdIndex = newCourseId.indexOf(notJoinedCourseId[0]);
            var notJoinedCourseName = newCourseName[notJoinedCourseIdIndex];
            bootbox.alert("User has either given feedback or not joined for Course - <b>" + notJoinedCourseName + "</b>.You Cannot add <b>" + notJoinedCourseName + " </b>.");
            Validate = false;
            return Validate;
        }

        //Checks if newly selected courseid and joined courseid(after removing course) are same
        var is_same = joinedCourseId.compare(newCourseId);

        if (!is_same) {
            bootbox.alert("Course mismatch");
            Validate = false;
            return Validate;
        }

        return Validate;
    }


    Array.prototype.compare = function (arr2) {
        // compare lengths - can save a lot of time
        this.sort();
        arr2.sort();
        if (this.length != arr2.length) {
            return false;
        }
        else {
            for (var i = 0, l = this.length; i < l; i++) {
                if (this[i] != arr2[i]) {
                    return false;
                }
            }

            return true;
        }

    }

    Array.prototype.diff = function (arr2) {
        var ret = [];
        this.sort();
        arr2.sort();
        for (var i = 0; i < arr2.length; i += 1) {
            if (this.indexOf(arr2[i]) == -1) {
                ret.push(arr2[i]);
            }
        }
        return ret;
    };

    Array.prototype.duplicate = function () {
        var arr = this;
        var sorted_arr = arr.slice().sort(); // You can define the comparing function here. 
        // JS by default uses a crappy string compare.
        // (we use slice to clone the array so the original array won't be modified)
        var results = [];
        for (var i = 0; i < arr.length - 1; i++) {
            if (sorted_arr[i + 1] == sorted_arr[i]) {
                results.push(sorted_arr[i]);
            }
        }

        return results;
    }

    var GetPaidAmountDetails = function () {
        var currSumPaidTotal = 0;
        var currSumCourseFeeTotal = 0;
        var currSTAmtPaidTotal = 0;
        var currFee = $("#txtFee").val();
        var stAmt = $("#txtSTAmt").val();
        var totalFee = $("#txtTotalFee").val();

        var $paidtr = $('input[value=True]').closest('tr');
        $($paidtr).find('.totalAmt').each(function () {
            currSumPaidTotal = currSumPaidTotal + Number($(this).val());
        });

        $($paidtr).find('.courseFee').each(function () {
            currSumCourseFeeTotal = currSumCourseFeeTotal + Number($(this).val());
        });

        $($paidtr).find('.stAmt').each(function () {
            currSTAmtPaidTotal = currSTAmtPaidTotal + Number($(this).val());
        });

        if (currFee == "0" && stAmt == "0" && totalFee == "0") {
            $("#txtFee").val(currSumCourseFeeTotal);
            $("#txtSTAmt").val(currSTAmtPaidTotal);
            $("#txtTotalFee").val(currSumPaidTotal);
        }
    }

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

            //if any one of the total amount is less than 100 except for the last row
            if (i != $tr.length - 1) {
               
                if (currTotalAmt < 100) {
                    validate = false;
                    bootbox.alert("Minimum amount should be 100");
                    $($tr[i]).find('.totalAmt').focus();
                    return validate;
                }
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

    $("#ddlMultiCourse").on("select2:select select2:unselect", function (e) {

        $("#txtDiscount").val("0");
        var courseId = $(this).val();
        //For GetCourseList calculation
        $("#txtMultiCourseId").val(courseId);
        var regId = $("#hFieldRegId").val();

        //Gets the last selected item
        var lastSelectedCourseId = e.params.data.id;

        var feeMode = $("input[name='InstallmentType']:checked").val();
        if (e.type == "select2:select") {

            GetCourseDetails(lastSelectedCourseId, feeMode, "add", regId);
        }
        else {

            GetCourseDetails(lastSelectedCourseId, feeMode, "subtract", regId);

        }


    });

    $(document).on("change", "#ddlCourseDowngradeId", GetCourseCodeDetails);

    //ddlRoundupList change function
    $(document).on("change", "#ddlRoundUpList", GetFeeDetails);

    //Show/Hide installment div 
    GetFeeModeDiv();

    //Gets the coursefee and performs calculation based on fee mode
    $(document).on('ifChecked', '.installment', GetCourseFee);

    $(document).on('change', '#ddlInstallmentNo', InstallmentChangeValidation);

    var ajaxUpdate = function () {

        var form = $("#frmAdd");
        var redirectUrl = $(form).data("redirect-url");
        $("#frmAdd").ajaxSubmit({
            iframe: true,
            dataType: "json",
            beforeSubmit: function () {
                $.blockUI({ message: '<h3><img src="../plugins/jQueryBlockUI/images/busy.gif" /> <b>Please wait... </b></h3>' });

            },
            success: function (result) {
                $.unblockUI();
                if (result.message == "success") {
                    toastr.success("Successfully saved the details.");
                    setTimeout(function () {
                        var url = $(form).data("redirect-url");
                        window.location.href = url;
                    }, 2000);
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
    };

    /////////////////////////////////PIN VERIFICATION ///////////////////////////////
    //it total fee is paid and if the no of rows are greater than the paid row count
    //then ask to change the installment to paid row count

    //var validateTableRows = function () {
    //    var $paidtr;
    //    var $totaltr;
    //    var currSumPaidTotal = 0;
    //    var currTotalAmount = 0;
    //    var currCourseFee = 0;
    //    var currSTAmount = 0;

    //    var totalAmount = Number($("#txtTotalFee").val());
    //    var currSTPercentage = Number($("#txtST").val());
    //    var currST = Number((100 + currSTPercentage) / 100);

    //    $paidtr = $('input[value=True]').closest('tr');
    //    $($paidtr).find('.totalAmt').each(function () {
    //        console.log(Number($(this).val()));
    //        currSumPaidTotal = currSumPaidTotal + Number($(this).val());
    //    });

    //    //if total amount has been paid
    //    if (currSumPaidTotal == totalAmount) {
    //        //Total tow count 
    //        $totaltr = $("input.totalAmt:text").closest('tr');
    //        //Paid row count is not equal to total row count the show alert
    //        if ($totaltr.length != $paidtr.length) {
    //            bootbox.alert("Please change the installment to <b>" + $paidtr.length + "</b>");
    //            return false;
    //        }

    //    }

    //    return true;
    //}

 


    var GetPinNo = function () {
        //blocking message goes here
        $('#divPinVerification').block({
            message: '<h5>Processing...</h5>',
            css: { border: '3px solid #a00' }
        });

        var href = $("#divPinVerification").data("url");
        var mobileNo = $("#hFieldMobNo").val();
        var data = $('#ddlMultiCourse').select2('data');
        var currCourseCode = "";
        for (var i = 0; i < data.length; i++) {
            if (currCourseCode == "") {
                currCourseCode = data[i].text;
            }
            else {
                currCourseCode = currCourseCode + "," + data[i].text;
            }
        }
        var currCourseFee = $("#txtFee").val();
        var prevCourseCode = $("#txtCurrMultiCourseCode").val();
        var prevCourseFee = $("#txtCurrFee").val();

        $.ajax({
            type: "GET",
            url: href,
            data: { studMobileNo: mobileNo, prevCourseFee: prevCourseFee, prevCourseCode: prevCourseCode, currCourseFee: currCourseFee, currCourseCode: currCourseCode },
            datatype: "json",
            success: function (data) {
                //if pinno is saved successfully
                if (data != 0) {
                    //unblocking processing message
                    $('#divPinVerification').unblock();
                    //storing the value of pinno in textbox
                    $("#txtReturnPinNo").val(data);
                }
                else {
                    $('#divPinVerification').unblock();
                    bootbox.alert("Cannot send pinno .Please verify students mobile.");
                    $(this).steps("previous");
                    $(this).steps("previous");
                    $(this).steps("previous");
                }

            },
            error: function (err) {

                toastr.error("Error:" + this.message)
            }
        });

    };


    var ValidatePinDetails = function () {
        var validate = false;
        if ($("#txtReturnPinNo").val() == $("#txtPinNo").val()) {
            validate = true;
        }
        else {
            bootbox.alert("Invalid PinNo");
        }
        return validate;
    };

    var GetManagerPinVerification = function () {
        //if pinno was already send then need not send again
        var returnPinNo = $("#mdlReturnPinNo").val();
        if (returnPinNo == "") {
            var href = $("#divModalPinVerification").data("url");
            var mobileNo = $("#hFieldMobNo").val();
            var data = $('#ddlMultiCourse').select2('data');
            var currCourseCode = "";
            for (var i = 0; i < data.length; i++) {
                if (currCourseCode == "") {
                    currCourseCode = data[i].text;
                }
                else {
                    currCourseCode = currCourseCode + "," + data[i].text;
                }
            }
            var currCourseFee = $("#txtFee").val();
            var prevCourseCode = $("#txtCurrMultiCourseCode").val();
            var prevCourseFee = $("#txtCurrFee").val();
            var studentName = $("#txtStudentName").val();
            var croName = $("#hFieldCroName").val();
            var mgrMobileNo = $("#hFieldMgrMobileNo").val();


            $.ajax({
                type: "GET",
                url: href,
                data: {
                    mgrMobileNo: mgrMobileNo, prevCourseFee: prevCourseFee, prevCourseCode: prevCourseCode,
                    currCourseFee: currCourseFee, currCourseCode: currCourseCode, studentName: studentName,
                    croName: croName
                },
                datatype: "json",
                success: function (data) {
                    //if pinno is saved successfully
                    if (data != 0) {
                        $("#mdlPinNo").val('');
                        $("#divMdlErrorPinNo").hide();
                        $("#mdlReturnPinNo").val(data);
                        $("#divModalPinVerification").modal({
                            backdrop: 'static'
                        });
                    }

                },
                error: function (err) {

                    toastr.error("Error:" + this.message)
                }
            });
        }
       //if pinno exists 
        else {
            $("#mdlPinNo").val('');
            $("#divMdlErrorPinNo").hide();            
            $("#divModalPinVerification").modal({
                backdrop: 'static'
            });
        }


        


    }

    $(document).on("click", "#btnContinue", function () {
        var returnPinNo = $("#mdlReturnPinNo").val();
        var currentPinNo = $("#mdlPinNo").val();

        if (currentPinNo != "") {
            if (returnPinNo == currentPinNo) {
                $("#divModalPinVerification").modal('hide');
                ajaxUpdate();
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
       

    })

});