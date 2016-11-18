
//REGISTRATION=>EDIT.JS

$(function () {
    var form = $("#frmEdit");
    form.validate({
        errorPlacement: function errorPlacement(error, element) { element.before(error); }
    });
    form.find("#RegistrationStepz").steps({
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

            //On clicking NEXT of Personal Data Tab
            if (currentIndex == 0) {
                if (form.valid()) {
                    if (PinVerification()) {
                        InstallmentValidation();
                        return true;
                    }
                    else {
                        return false;
                    }
                }
                // return form.valid();
            }

            //if the tab is payment details
            if (newIndex == 2) {
                if (InstallmentChangeValidation()) {
                    InitPaymentTable();
                    return true;
                }
                else {
                    return false;
                }

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
            ajaxUpdate();

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

    $("#ddlRoundUpList").select2();

    $("#ddlInstallmentNo").select2({
        placeholder: "Select no of Installment",
        allowClear: true
    });

    //dropdown search performed here
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

    //Validate email click function
    $(document).on("click", "#validateEmailLink", function () {
        var box = bootbox.confirm("Validation link will be send to students emailid once again.Click OK to proceed", function (result) {
            if (result) {
                box.modal('hide');
                $.blockUI({ message: null });

                var studId = $("#hFieldStudentRegId").val();
                var studRegId = $("#txtRegistrationID").val();
                var studName = $("#txtStudentName").val();
                var courseList = $("#txtMultiCourseCode").val();
                var courseFee = Number($("#txtFee").val());
                var studEmailId = $("#validateEmailLink").data("email");
                var href = $("#validateEmailLink").data("url");

                $.ajax({
                    type: "POST",
                    url: href,
                    data: { studId: studId, studRegId: studRegId, studName: studName, courseList: courseList, courseFee: courseFee, studEmailId: studEmailId },
                    datatype: "json",
                    success: function (data) {

                        $.unblockUI();
                        if (data == "success") {
                            bootbox.alert("Successfully send the email.");
                        }
                        else if (data == "mail_error") {
                            bootbox.alert("Error while sending mail.");
                        }
                        else {
                            bootbox.alert("Error:Something gone wrong");
                        }
                    },
                    error: function (data) {
                        bootbox.alert("Exception:Something gone wrong");
                    }
                });
            }



        });


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

    //Show/Hide installment div 
    var GetFeeModeDiv = function () {
        var feeMode = $("input[name='InstallmentType']:checked").val();
        if (feeMode == "SINGLE") {
            $("#divInstallmentNo").slideUp();
        }
        else {
            $("#divInstallmentNo").slideDown();
        }

        //Disabling registration venue section
        $('.regnVenue').iCheck('disable');

        //Enabling/Disabling installment radiobutton    
        var count = $('input[value=True]').closest('tr').length;
        if (count >= 2) {
            $('.installment').iCheck('disable');
        }
    }

    GetFeeModeDiv();

    //hide/show installment section based on installmentradiobutton change
    $(document).on('ifChecked', '.installment', function (e) {
        var instType = $(this).val();

        if ($("#ddlMultiCourse").val() != null) {
            if (instType == "SINGLE") {
                $("#divInstallmentNo").slideUp();
            }
            else {
                $("#divInstallmentNo").slideDown();
            }

            GetCourseFee(instType);
        }


    });

    //setting multicourseid value for  dropdown coursecode search
    var selectedCourseId = $("#ddlMultiCourse").val();
    $("#txtMultiCourseId").val(selectedCourseId);

    //Payment table creation is done here
    var InitPaymentTable = function () {

        GetCallOutMessage();
        GetDataTable();
        GetPaymentDetailsOnTxtbxChange();
        GetDatepicker();
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

    var GetCourseFee = function (feeMode) {

        var href = $("#divFeeMode").data("url");
        var multiCourseId = $("#txtMultiCourseId").val();
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

    };

    //calculation of fees on course selection/unselection/discount/roundup is performed here
    var GetFeeDetails = function () {
        //ST Percentage when course is joined
        var dbST = Number($("#hFieldDbST").val());
        //Gets the latest ST
        var currST = Number($("#txtST").val());

        //If there is no change in ST
        if (dbST == currST) {
            //Gets the current discount percentage
            var currDiscount = parseInt($("#txtDefaultDiscountPercentage").val());
            //Gets the current fee
            var courseFee = parseInt($("#txtCourseFee").val());
            //Gets the current ST percentage
            var currST = Number($("#txtST").val());

            //TotalAmount Calculation
            var stAmt = Math.round((courseFee) * (currST / 100));
            //current total amount is calculating total amount without applying discount
            var currTotalAmt = courseFee + stAmt;


            //Gets the new discount amount
            var discountAmt = Math.round((currTotalAmt) * (currDiscount / 100));
            //current total amount is calculating total amount by applying discount
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

            
            var courseInterchangeFee = $.isNumeric(parseInt($("#txtCourseInterchangeFee").val())) ? parseInt($("#txtCourseInterchangeFee").val()) : 0;
            var courseInterchangeST = $.isNumeric(parseInt($("#txtCourseInterchangeST").val())) ? parseInt($("#txtCourseInterchangeST").val()) : 0;

            //Adding courseinterchange fee 
            newTotalAmt = newTotalAmt + courseInterchangeFee + courseInterchangeST;


            $("#txtFee").val(courseFee);
            $("#txtSTAmt").val(newSTAmt);
            $("#txtTotalFee").val(newTotalAmt);
            $("#txtTotalAmt").val(currTotalAmt);//hidden field

            return false;
        }
            //if there is change in ST
        else {

            //Gets the current discount percentage
            var currDiscount = parseInt($("#txtDefaultDiscountPercentage").val());
            //Gets the current fee
            var actualCourseFee = parseInt($("#txtCourseFee").val());
            //Gets the current ST percentage
            var currST = Number($("#txtST").val());

            //CourseFee after applying discount
            var newCourseFee = Math.round((actualCourseFee * ((100 - currDiscount) / 100)));

            //Gets the paid coursefee,st and total amount details
            var paidCourseFee = parseInt($("#hFieldTotalCourseFeePaid").val());
            var paidST = parseInt($("#hFieldTotalSTPaid").val());
            var paidTotalAmount = parseInt($("#hFieldTotalAmountPaid").val());

            //CourseFee after reducing paid courseFee amount
            var unpaidCourseFee = newCourseFee - paidCourseFee;
            var unpaidST = Math.round(unpaidCourseFee * (currST / 100));
            var unpaidTotalAmount = Math.round(unpaidCourseFee + unpaidST);

            //Getting current coursefee,st,totalamount
            var currCourseFee = paidCourseFee + unpaidCourseFee;
            var currSTAmt = paidST + unpaidST;
            var currTotalAmount = paidTotalAmount + unpaidTotalAmount;

            var courseInterchangeFee = parseInt(("#txtCourseInterchangeFee").val());
            var courseInterchangeST = parseInt(("#txtCourseInterchangeST").val());

            //Adding courseinterchange fee 
            currTotalAmount = currTotalAmount + courseInterchangeFee + courseInterchangeST;

            $("#txtFee").val(currCourseFee);
            $("#txtSTAmt").val(currSTAmt);
            $("#txtTotalFee").val(currTotalAmount);
            $("#txtTotalAmt").val(currTotalAmount);
        }
    };

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


                //Clearing all the dbTotal amount
                $($tr).find('.dbTotalAmt').each(function () {
                    $(this).val('');
                });


            }
                //Installment upgradation
            else {
                AddRows(gridRowLength, currSelectedRowLength);

                //Clearing all the dbTotal amount
                $($tr).find('.dbTotalAmt').each(function () {
                    $(this).val('');
                });

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


            //Clearing all the dbTotal amount
            $($tr).find('.dbTotalAmt').each(function () {
                $(this).val('');
            });
        }
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

            $row.append(' <td style="display:none"><input name="StudentReceipt[' + i + '].Total" type="text" class="form-control dbTotalAmt numberOnly"  />'
                     + '</td>');

            $row.append(' <td><input name="StudentReceipt[' + i + '].DueDate" type="text" class="form-control valid dueDate date" data-val="true" data-val-required="Enter DueDate" placeholder="DUEDATE" id="txtDueDate' + i + '" readonly="readonly" style="background:white" />'
                            + '<span class="field-validation-valid " data-valmsg-for="StudentReceipt[' + i + '].DueDate" data-valmsg-replace="true"></span> </td>');

            $tbody.append($row);
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
        var dbTotal = $tbody.find('.dbTotalAmt');
        var stPercentage = $tbody.find('.stPercentage');
        var currSTPercent = Number($("#txtST").val());

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
            //getting the current DbTotal amount
            var dbTotalAmt = $(dbTotal.eq(index)).val();

            //calculating coursefee
            currCourseFee = Math.round(currTotalAmt / totalSTPercent);
            //calculating st amount
            currSTAmt = parseInt(currTotalAmt - currCourseFee);

            //setting the value of coursefee
            $(totalCourseFee.eq(index)).val(currCourseFee);
            //setting the value of stamt
            $(totalSTAmt.eq(index)).val(currSTAmt);

            ///////////Next Row Calculation////////////
            //check if dbTotal amount exist
            if (dbTotalAmt != "") {
                dbTotalAmt = Number(dbTotalAmt);
                var currTotal = Number($(this).val());


                //if entered total is less than the dbTotal amount
                if (currTotal < dbTotalAmt) {
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

                }

                    //if entered total is greater than the dbTotal amount
                else if (currTotal > dbTotalAmt) {
                    //Gets the length of the row
                    var lstRow = $("#tblPaymentDetails tbody tr").length - 1;
                    //gets the difference amount
                    var sumAmount = GetTotalAmtExceptCurrentRow(lstRow);
                    //calculates the lastrow totalamount
                    var lstTotalAmt = totalFee - sumAmount;
                    //calculates the lastrow coursefee
                    var lstCourseFee = Math.round(lstTotalAmt / totalSTPercent);
                    //calculates the lastrow stamount
                    var lstSTAmt = parseInt(lstTotalAmt - lstCourseFee);

                    //setting the value of nextrow coursefee
                    $(totalCourseFee.eq(lstRow)).val(lstCourseFee);
                    //setting the value of nextrow stamount
                    $(totalSTAmt.eq(lstRow)).val(lstSTAmt);
                    //setting the value of nextrow totalamount
                    $(totalAmount.eq(lstRow)).val(lstTotalAmt);
                    //setting the ST Percentage
                    $(stPercentage.eq(lstRow)).val(currSTPercent);

                }

                    //else if current amount and dbTotal amount are equal
                else {
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
                }
            }
            else {
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
            }


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

    //Pin verification on mobile change
    var PinVerification = function () {
        var form = $("#frmEdit");
        if (form.valid()) {
            var validate = false;
            var currMobNo = $("#txtMobileNo").val();
            var defaultMobNo = $("#txtDefaultMobNo").val();
            if (currMobNo != defaultMobNo) {
                var box = bootbox.confirm("MobileNo has changed.Requires pin verification.Click <b>OK</b> to proceed.", function (result) {
                    if (result) {
                        box.modal('hide');
                        $.blockUI({ message: null });

                        var href = $("#divModalPinVerification").data("url");
                        var newMobNo = $("#txtMobileNo").val();

                        $.ajax({
                            type: "GET",
                            url: href,
                            data: { studMobileNo: newMobNo },
                            datatype: "json",
                            success: function (data) {
                                //if pinno is saved successfully
                                if (data != 0) {
                                    validate = true;
                                    $.unblockUI();
                                    $("#mdlPinNo").val('');
                                    $("#divMdlErrorPinNo").hide();
                                    $("#mdlReturnPinNo").val(data);

                                    $("#divModalPinVerification").modal({
                                        backdrop: 'static'
                                    });
                                }
                                else {
                                    $('#divPinVerification').unblock();
                                    bootbox.alert("Cannot send pinno .Please verify students mobile.");
                                }

                            },
                            error: function (err) {
                                bootbox.alert("Error:" + this.message);
                            }
                        });
                    }
                });
            }
            else {
                validate = true;
            }
            return validate;
        }
    }

    //if full fees has been paid then disable installment change
    var InstallmentValidation = function () {
        var $trCount = $("#tblPaymentDetails tbody tr").length;
        var paidCount = $('input[value=True]').closest('tr').length;
        if ($trCount == paidCount) {
            $("#ddlInstallmentNo").prop("disabled", true);

        }
        else {
            $("#ddlInstallmentNo").prop("disabled", false);
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

        if (validate) {
            return DateComparison();
        }

        return validate;
    }

    //Datecomparison of the duedate
    var DateComparison = function () {
        var validate = true;
        var $tr = $("#tblPaymentDetails tbody tr");

        $($tr).each(function () {
            var current_date = new Date($(this).find('.date').val());// or whatever you call your date cell
            var next_date = new Date($(this).next().find('.date').val()) // also strip these evaluations from the time as you described...
            if (current_date > next_date) {
                var errorIndex = $(this).next().index();

                //showing error message on duedate mismatch
                bootbox.alert("DueDate mismatch on row " + (errorIndex + 1) + "", function () {
                    var $td = $tr.eq(errorIndex).find('.date').closest('td');
                    $($td).append("<span class='duedate-mismatch field-validation-error'>DueDate Mismatch</span>")
                    setTimeout(function () {
                        $('.duedate-mismatch').fadeOut(1500);

                    }, 3000);
                });

                validate = false;
                return validate;
            }
        });
        return validate;
    }

    $(document).on("click", "#btnContinue", function () {
        var returnPinNo = $("#mdlReturnPinNo").val();
        var currentPinNo = $("#mdlPinNo").val();

        if (returnPinNo == currentPinNo) {
            var currMobNo = $("#txtMobileNo").val();
            $("#txtDefaultMobNo").val(currMobNo);
            $("#divModalPinVerification").modal('hide');
        }
        else {
            $("#divMdlErrorPinNo").show();
        }

    })

    $(document).on("click", "#btnMdlClose", function () {
        var defaultMobNo = $("#txtDefaultMobNo").val();
        $("#txtMobileNo").val(defaultMobNo);

    })

    var ajaxUpdate = function () {

        var form = $("#frmEdit");
        var redirectUrl = $(form).data("redirect-url");
        var receiptUrl = $(form).data("receipt-redirect-url");
        $("#frmEdit").ajaxSubmit({
            iframe: true,
            dataType: "json",
            beforeSubmit: function () {
                $.blockUI({ message: '<h3><img src="../plugins/jQueryBlockUI/images/busy.gif" /> <b>Please wait... </b></h3>' });

            },
            success: function (result) {
                $.unblockUI();
                if (result.message == "success") {
                    toastr.success("Successfully saved the details.");
                    bootbox.confirm("Do you want to redirect to <b>Receipt</b> page ?", function (result) {
                        if (result) {
                            window.location.href = receiptUrl;
                        }
                        else {
                            window.location.href = redirectUrl;
                        }

                    });
                }
                else if (result.message == "error_mobile") {
                    toastr.error("Error while sending registration sms student")
                }
                else if (result.message == "error_email") {
                    toastr.error("Error while sending email to student")
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

    $("#ddlInstallmentNo").change(function () {
        InstallmentChangeValidation();
    });

    //validating paymentcount and installment change(user cannot change installment <= paymentcount)
    var InstallmentChangeValidation = function () {

        var validate = true;
        var paidCount = $('input[value=True]').closest('tr').length;
        var currInstallment = $("#ddlInstallmentNo").val();
        var remainingPaidCount = $('input[value=False]').closest('tr').length;
        //if installment was chosen else currInstallment will be ""
        if (currInstallment != "" && remainingPaidCount > 0) {


            if (currInstallment <= paidCount) {
                validate = false;
                bootbox.alert("<b>" + paidCount + "</b> payments are completed.So installment cannot be changed to <b>" + currInstallment + "</b>")
            }
        }
        return validate;
    };


});