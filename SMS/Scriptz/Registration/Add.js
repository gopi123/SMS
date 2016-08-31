//REGISTRATION ==> ADD.JS
//update1:Added else condition if currentindex=2 and form is not valid
$(function () {

    var form = $("#frmAdd");
    form.validate({
        errorPlacement: function errorPlacement(error, element) { element.before(error); }
    });
    form.find("#WalkInnStepz").steps({
        headerTag: "h3",
        bodyTag: "section",
        transitionEffect: "slideLeft",
        onStepChanging: function (event, currentIndex, newIndex) {
            $(form).data('validator', null);
            $.validator.unobtrusive.parse($('form'));
            form.validate().settings.ignore = ":disabled,:hidden";

            if (currentIndex > newIndex) {
                return true;
            }

            //if the tab is payment details
            if (newIndex == 2) {
                InitPaymentTable();
                return form.valid()
            }
            //function to call on clicking next of payment details tab
            if (currentIndex == 2) {
                if (form.valid()) {
                    if (ResetFeeDetails()) {
                        GetPinNo();
                    }
                    else {
                        return false;
                    }

                }
                else {
                    return false;
                }
            }

            return true;
        },
        onFinishing: function (event, currentIndex) {
            if (form.valid()) {
                return ValidatePinDetails();
            }
            return form.valid();
        },
        onFinished: function (event, currentIndex) {
            $(this).steps("previous");
            $(this).steps("previous");
            $(this).steps("previous");
            ajaxInsert();

        }
    });

    $("#txtDiscount").tooltip({ placement: 'left' });

    $("#txtPinNo,#mdlPinNo").inputmask("9999");

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

    //fileUpload plugin
    $(".imgUpload").fileinput({
        showUpload: false

    });

    $("#ddlInstallmentNo").select2({
        placeholder: "Select no of Installment",
        allowClear: true
    });


    $("#ddlCourseList").select2({
        placeholder: "Select CourseCode",
        allowClear: true
    });

    $("#ddlRoundUpList").select2();

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

    //hide/show installment section based on installmentradiobutton change
    $(document).on('ifChecked', '.installment', function (event) {
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

    });

    //Discount calculation is performed here
    $("#txtDiscount").change(function () {
        smsVerificationOnDiscountChange();
        GetFeeDetails();

    });

    //ddlRoundupList change function
    $(document).on("change", "#ddlRoundUpList", function () {

        GetFeeDetails();

    });

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
                           + '<span class="field-validation-valid spanPartnerName" data-valmsg-for="StudentReceipt[' + i + '].Fee" data-valmsg-replace="true"></span> </td>');

            $row.append(' <td><input name="StudentReceipt[' + i + '].ST" type="text" class="form-control valid stAmt" data-val="true" data-val-required="ST required" placeholder="ST" readonly="readonly"  />'
                           + '<span class="field-validation-valid spanContactNumber" data-valmsg-for="StudentReceipt[' + i + '].ST" data-valmsg-replace="true"></span> </td>');

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
            var dueDate = $($tr[0]).find('.dueDate').val().split('/');
            var errorIndex = $($tr[i]).index();
            
            //if any one of the total amount is less than 100
            if (currTotalAmt < 100) {
                validate = false;
                bootbox.alert("Minimum amount should be 100");
                $($tr[i]).find('.totalAmt').focus();
                return validate;
            }

            //Checking if the first payment is the current date
            var currDate = new Date();
            dueDate = new Date(dueDate[2], dueDate[0] - 1, dueDate[1]);            
            if (currDate.toDateString() != dueDate.toDateString()) {
                validate = false;

                bootbox.alert("First Payment Date should be Todays date", function () {
                    var $td = $tr.eq(errorIndex).find('.dueDate').closest('td');
                    $($td).append("<span class='duedate-mismatch field-validation-error'>Enter todays date </span>")
                    setTimeout(function () {
                        $('.duedate-mismatch').fadeOut(1500);

                    }, 3000);
                });               
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

    var GetPinNo = function () {
        //blocking message goes here
        $('#divPinVerification').block({
            message: '<h5>Processing...</h5>',
            css: { border: '3px solid #a00' }
        });

        var href = $("#divPinVerification").data("url");
        var mobileNo = $("#txtMobileNo").val();
        $.ajax({
            type: "GET",
            url: href,
            data: { studMobileNo: mobileNo },
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

                $("#divModalReceiptDetails").modal({
                    backdrop: 'static'
                });

                bootbox.hideAll()
                return false;
               
            }

        });


    };


    

    $(document).on("click", "#btn_Registration_PayNow", function () {
        var form = $("#frmAdd");


        //show alert if confirm payment checkbox has not been checked
        var isPaymentConfirmed = $("#chkbxConfirm_Receipt").prop("checked");
        if (isPaymentConfirmed) {

            $("#divModalReceiptDetails").modal('hide');

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

                        var href = $("#divModalPinVerification").data("url");
                        var walkinnId = $("#hfieldWalkInnID").val();
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
                                    $("#mdlPinNo").val('');
                                    $("#divMdlErrorPinNo").hide();
                                    $("#mdlReturnPinNo").val(data);
                                    $.unblockUI();
                                    $("#divModalPinVerification").modal({
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


    $(document).on("click", "#btnContinue", function () {
        var returnPinNo = $("#mdlReturnPinNo").val();
        var currentPinNo = $("#mdlPinNo").val();

        if (returnPinNo == currentPinNo) {
            var currDiscountPercentage = $("#txtDiscount").val();
            $("#txtDefaultDiscountPercentage").val(currDiscountPercentage);
            $("#divModalPinVerification").modal('hide');
            GetFeeDetails();
        }
        else {
            $("#divMdlErrorPinNo").show();
        }

    })

    $(document).on("click", "#btnMdlClose", function () {
        $("#txtDiscount").val(0);
        GetFeeDetails();

    })

    ////////////////////////////////////Course Code Search//////////////////////////////////////////////////////


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