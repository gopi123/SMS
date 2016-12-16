//CourseDowngrade ====> Add.js

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

            if (form.valid()) {

                //On clicking NEXT of Course Details
                if (currentIndex == 1) {
                    if (!ValidateCourse()) {
                        return false;
                    }
                }
                //if the tab is payment details
                if (newIndex == 2) {
                    InitPaymentTable();

                }

                //if the tab is pinverification
                if (newIndex == 3) {

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
                if (ValidatePinDetails()) {
                    return true;
                }
                else {
                    var $span = $("#returnMsg");
                    $span.text("Invalid PinNo")

                    setTimeout(function () {
                        $('#returnMsg').fadeOut(1500);

                    }, 3000);
                }

            }
        },
        onFinished: function (event, currentIndex) {
            ajaxUpdate();

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

    $("#ddlCourseInterchangeId").select2({
        placeholder: "Select course to downgrade"
    });

    $("#ddlMultiCourse").select2({
        placeholder: "Select CourseCode"
    });

    $("#ddlCourseList").select2({
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

    //Showing and Hiding installment div +
    //Enabling/Disabling installment radiobutton    
    var GetFeeModeDiv = function () {

        var feeMode = $("input[name='InstallmentType']:checked").val();
        if (feeMode == "SINGLE") {
            $("#divInstallmentNo").slideUp();
        }
        else {

            $("#divInstallmentNo").slideDown();
        }

        $('.installment').iCheck('disable');

    }


    //Show/Hide installment div on pageload
    GetFeeModeDiv();

    //GetCourseDetails on installment change and coursecode change
    GetCourseDetails = function () {
        var multiCourseId = $("#ddlMultiCourse").val();
        var href = $("#divCourseCode").data("details-url");
        var regId = $("#hFieldRegId").val();
        var feeMode = $("input[name='InstallmentType']:checked").val();

        $.ajax({
            type: "GET",
            url: href,
            data: { multiCourseId: multiCourseId, feeMode: feeMode, regId: regId },
            datatype: "json",
            traditional: true,
            success: function (data) {
                //$(spinner).toggle(false);
                //enabling discount textbox if newcoursefee > paidfee

                var courseFee = parseInt(data[0].CourseFee);
                var paidFee = $("#hFieldTotalFeePaid").val();
                var isSingleAndFullyPaid = data[0].IsSingleAndFullyPaid;


                if (courseFee > paidFee) {

                    if (isSingleAndFullyPaid && feeMode == "SINGLE") {
                        var rbtnSingle = $(":radio[value=SINGLE]");
                        var rbtnInstallment = $(":radio[value=INSTALLMENT]");
                        $(rbtnInstallment).iCheck('check');
                        $(rbtnSingle).iCheck('uncheck');

                        GetCourseDetails();
                        return;
                    }

                    $("#txtDiscount").attr("readonly", false);
                    $("#txtMaxDiscountAllowed").val(data[0].MaxAllowedDiscount)
                }
                else {
                    $("#txtDiscount").attr("readonly", true);
                }

                $("#txtCourseTitle").val(data[0].CourseTitle);
                $("#txtSoftwareUsed").val(data[0].SoftwareUsed);
                $("#txtDuration").val(data[0].Duration);
                $("#txtMultiCourseCode").val(data[0].CourseCode);
                $("#txtSingleCourseIds").val(data[0].CourseIds);//used for validating course

                $("#txtDiscount").val(data[0].DiscountPercentage);
                $("#txtFee").val(data[0].CourseFee);
                $("#txtCourseFee").val(data[0].CourseFee);//for discount calculation
                $("#txtSTAmt").val(data[0].STAmount);

                //calculating new courseinterchangefee
                var newCourseFee = parseInt($("#txtFee").val());
                var newSTAmt = parseInt($("#txtSTAmt").val());

                var courseInterchangeFee = $.isNumeric(parseInt($("#txtCourseInterchangeFee").val())) ?
                                           parseInt($("#txtCourseInterchangeFee").val()) : 0;
                var courseInterchange_ST_Fee = $.isNumeric(parseInt($("#txtCourseInterchangeST").val())) ?
                                              parseInt($("#txtCourseInterchangeST").val()) : 0;

                //new total fee
                var newTotalFee = parseInt(newCourseFee + newSTAmt + courseInterchangeFee + courseInterchange_ST_Fee);

                $("#txtTotalFee").val(newTotalFee);
            },
            error: function (err) {
                //$(spinner).toggle(false);
                toastr.error("Error:" + this.message)
            }
        });

    };

    //Gets the coursedetails and performs calculation based on fee mode
    $(document).on('ifChecked', '.installment', function () {

        var feeMode = $(this).val();
        //hide or show installment div
        if (feeMode == "SINGLE") {
            $("#divInstallmentNo").slideUp();
        }
        else {
            $("#divInstallmentNo").slideDown();
        }

        //GetCourseDetails();




    });

    //Coursecode search
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
                    value: $("#txtMultiCourseId").val()
                };
            },
            processResults: function (data, params) {

                return {
                    results: data

                };
            }

        }
    });

    $("#ddlMultiCourse").on("select2:select select2:unselect", function (e) {
        //For GetCourseList calculation
        var courseId = $(this).val();
        $("#txtMultiCourseId").val(courseId);
        GetCourseDetails();

    });

    //ddlRoundupList change function
    //$(document).on("change", "#ddlRoundUpList", GetFeeDetails);

    //Below function makes sure that user has not selected more courses
    //other than the students course   
    var ValidateCourse = function () {

        var validate = true;
        var _feedbackCourseIds = $("#hFieldFeedbackCourseId").val().split(',').filter(function (v) { return v !== '' });;
        var _newCourseIds = $("#txtSingleCourseIds").val().split(',');
        //taking count of already joined courses
        var joinedCourseCombo = $("#txtCurrSoftwareUsed").val().split("\n");

        //Selected course to downgrade
        var newCourseCombo = $("#txtSoftwareUsed").val().split("\n");

        //validating feedback courses
        //Checking whether the new combiation contains feedback course
        if (_feedbackCourseIds.length == 0 || _feedbackCourseIds.contains(_newCourseIds)) {
            validate = true;
        }
        else {
            validate = false;
            bootbox.alert("Feedback courses is not present.Kindly add the feedback course.");
            return validate;
        }

        //validating discount allowed
        var _allowedDiscountPercentage = $("#txtMaxDiscountAllowed").val();
        var _givenDiscountPercentage = $("#txtDiscount").val();
        var _feePaid = parseInt($("#hFieldTotalFeePaid").val());
        var _courseFee = parseInt($("#txtFee").val());

        if (_givenDiscountPercentage > _allowedDiscountPercentage) {
            validate = false;
            bootbox.alert("Maximum allowed discount percentage is <b>" + _allowedDiscountPercentage + "%</b>.Cannot give more discount.");
            return validate;
        }

        return validate;
    }

    //checks if array1 has all items in array2
    Array.prototype.contains = function (arr2) {
        // compare lengths - can save a lot of time
        this.sort();
        arr2.sort();
        for (var i = 0, l = this.length; i < l; i++) {
            if (arr2.indexOf(this[i]) === -1) {
                return false;
            }
        }

        return true;

    }



    //Discount calculation is performed here
    $("#txtDiscount").change(function () {

        GetFeeDetailsOnDiscountChange();

    });

    //calculation of fees on course selection/unselection/discount/roundup is performed here
    var GetFeeDetailsOnDiscountChange = function () {

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
        newTotalAmt = Math.ceil(newTotalAmt / 10) * 10;


        //calculating coursefee from new total amt
        courseFee = Math.round((newTotalAmt) / ((100 + currST) / 100));
        //Gets the new st amt
        var newSTAmt = parseInt(newTotalAmt - courseFee);

        var courseInterchangeFee = $.isNumeric(parseInt($("#txtCourseInterchangeFee").val())) ?
                                   parseInt($("#txtCourseInterchangeFee").val()) : 0;
        var courseInterchange_ST_Fee = $.isNumeric(parseInt($("#txtCourseInterchangeST").val())) ?
                                      parseInt($("#txtCourseInterchangeST").val()) : 0;

        //new total fee
        var newTotalFee = parseInt(courseFee + newSTAmt + courseInterchangeFee + courseInterchange_ST_Fee);

        $("#txtTotalFee").val(newTotalFee);
        $("#txtFee").val(courseFee);
        $("#txtSTAmt").val(newSTAmt);
        $("#txtTotalFee").val(newTotalFee);


        return false;
    };


    ////////////////////////////////////////Payment Tab ///////////////////////////////////////////////
    //Gets the callout message of payment details
    var GetCallOutMessage = function () {

        var courseInterchangeFee = $.isNumeric(parseInt($("#txtCourseInterchangeFee").val())) ? parseInt($("#txtCourseInterchangeFee").val()) : 0;
        var courseInterchangeST = $.isNumeric(parseInt($("#txtCourseInterchangeST").val())) ? parseInt($("#txtCourseInterchangeFee").val()) : 0;
        var fee = parseInt($("#txtFee").val()) + courseInterchangeFee;
        var st = parseInt($("#txtSTAmt").val()) + courseInterchangeST;
        var totalFee = "TotalFee - &#8377; " + fee;
        var totalST = "TotalST - &#8377; " + st;
        var totalAmt = "TotalAmount - &#8377; " + $("#txtTotalFee").val();
        $("#titleTotalamt").html(totalFee + ' ' + totalST + ' ' + totalAmt);
    };

    var AddRows = function (start, end) {

        var $tbody = $("#tblPaymentDetails tbody");
        for (var i = start; i < end; i++) {
            var slno = parseInt(i + 1);
            var $row = $('<tr/>');
            $row.append('<input class="status" id="txtStatus_0"  name="StudentReceiptLists[' + i + '].Status" type="hidden" value="False">')

            $row.append(' <td class="slno">' + slno + '</td>');

            $row.append(' <td><input name="StudentReceiptLists[' + i + '].ReceiptNo" type="text" class="form-control receiptno" placeholder="RECEIPTNO" readonly="readonly" />'
                         + '</td>');

            $row.append(' <td><input name="StudentReceiptLists[' + i + '].Fee" type="text" class="form-control courseFee" placeholder="FEE" readonly="readonly" />'
                           + '</td>');

            $row.append(' <td><input name="StudentReceiptLists[' + i + '].STPercentage" type="text" class="form-control stPercentage" placeholder="ST PERCENTAGE" readonly="readonly" id="txtSTPercentage_"' + i + ' />'
                          + '</td>');

            $row.append(' <td><input name="StudentReceiptLists[' + i + '].ST" type="text" class="form-control stAmt" placeholder="ST" readonly="readonly"  />'
                           + '</td>');

            $row.append(' <td><input name="StudentReceiptLists[' + i + '].Total" type="text" class="form-control valid totalAmt numberOnly" data-val="true" data-val-required="Enter Total" placeholder="TOTAL"  />'
                            + '<span class="field-validation-valid " data-valmsg-for="StudentReceiptLists[' + i + '].Total" data-valmsg-replace="true"></span> </td>');


            $row.append(' <td><input name="StudentReceiptLists[' + i + '].DueDate" type="text" class="form-control valid dueDate date" data-val="true" data-val-required="Enter DueDate" placeholder="DUEDATE" id="txtDueDate' + i + '" readonly="readonly" style="background:white" />'
                            + '<span class="field-validation-valid " data-valmsg-for="StudentReceiptLists[' + i + '].DueDate" data-valmsg-replace="true"></span> </td>');

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
        if (currSelectedRowLength >= gridRowLength) {

            $tr = $("#tblPaymentDetails tbody tr");

            //Adding FeeDetails
            $($tr).find('.totalAmt').each(function () {
                currSumTotal = currSumTotal + Number($(this).val());
            });

            //Single to installment
            if (currSumTotal != totalAmount) {
                if (currSelectedRowLength > gridRowLength) {
                    AddRows(gridRowLength, currSelectedRowLength)
                }


                //Calculating the sum of  paidRows
                $paidtr = $('input[value=True]').closest('tr');
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
        GetDataTable();
        GetPaymentDetailsOnTxtbxChange();
        GetDatepicker();
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
            //$("#txtFee").val(totalCourseFee);
            //$("#txtSTAmt").val(totalST);
            //GetCallOutMessage();

        }
        else {
            validate = false;
            bootbox.alert("TotalAmount Mismatch");
        }

        return validate;
    }







    /////////////////////// Data post into the server ///////////////////////////////////////


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

                if (result.Status == "success") {
                    sendEmail(result.Id);
                }
                else {
                    $.unblockUI();
                    toastr.error("Error:Something gone wrong")
                }
            },
            error: function (xhr, textStatus, errorThrown) {
                $.unblockUI();
                toastr.error("Error:Something gone wrong")
            }

        });
    };

    var sendEmail = function (studRegId) {
        var href = $("#frmAdd").data("mail-send-url");
        var isEmailSendToCentreHead = true;
        $.ajax({
            type: "GET",
            url: href,
            data: { regId: studRegId, isEmailSendToCentreHead: isEmailSendToCentreHead },
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

    /////////////////////////////////PIN VERIFICATION ///////////////////////////////

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
        var currCourseFee = parseInt($("#txtFee").val());
        var prevCourseCode = $("#txtCurrMultiCourseCode").val();
        var prevCourseFee = $("#txtCurrFee").val();
        var studentID = $("#txtStudentID").val();
        var studentName = $("#txtStudentName").val();
        var courseInterchangeFee = $.isNumeric(parseInt($("#txtCourseInterchangeFee").val())) ? parseInt($("#txtCourseInterchangeFee").val()) : 0;
        var totalCourseFee = currCourseFee + courseInterchangeFee;

        $.ajax({
            type: "GET",
            url: href,
            data: {
                studMobileNo: mobileNo, prevCourseFee: prevCourseFee, prevCourseCode: prevCourseCode, currCourseFee: totalCourseFee, currCourseCode: currCourseCode,
                studentID: studentID, studentName: studentName
            },
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
            validate = false;
        }
        return validate;
    };

    ////////////////////////COURSE CODE SEARCH //////////////////////////////////////////

    var showModalCourseCode = function () {
        $("#ddlCourseList").select2("val", "");
        $(".divCourseCodeContent").html('');
        $("#divModalCourseCodeSearch").modal({
        });
    }

    var srchCourseCode = function () {
        var courseIds = new Array();
        courseIds = $("#ddlCourseList").val();
        var href = $("#divModalCourseCodeSearch").data("url");
        $(".spinner").show();
        $(".divCourseCodeContent").slideUp();
        $.ajax({
            type: "GET",
            url: href,
            data: { courseId: courseIds },
            datatype: "html",
            traditional: true,
            success: function (data) {
                $(".spinner").hide();
                $(".divCourseCodeContent").html(data);
                $(".divCourseCodeContent").slideDown();
            },
            error: function (err) {
                $(".spinner").hide();
                toastr.error("Error:" + this.message)
            }
        });


        return false;

    }

    $(document).on("click", "#btnSearchCourseCode", showModalCourseCode);

    $(document).on("click", "#btnCourseSearch", srchCourseCode);


});