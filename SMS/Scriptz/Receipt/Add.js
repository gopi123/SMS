//RECEIPT => ADD.JS

$(function () {
    var form = $("#frmAdd");
    form.validate({
        errorPlacement: function errorPlacement(error, element) { element.before(error); }
    });
    form.find("#ReceiptStepz").steps({
        headerTag: "h3",
        bodyTag: "section",
        transitionEffect: "slideLeft",
        onStepChanging: function (event, currentIndex, newIndex) {

            if (currentIndex > newIndex) {
                return true;
            }

            if (newIndex == 2) {
                //hiding the finish link
                var $finishLink = $('a[href="#finish"]');
                $($finishLink).hide();

                GetCallOutMessage();
                DisablePayButtonExceptFirstUnpaid();
                Check_DueDate_EmailValidation();
                PaymentTypeValidation();
            }
            return true;

        },
        onFinishing: function (event, currentIndex) {

        },
        onFinished: function (event, currentIndex) {
            $(this).steps("previous");
            $(this).steps("previous");
            $(this).steps("previous");


        }
    });



    //text-input to uppercase
    $(".form-control").addClass('capitalise');

    //toastr options
    toastr.options = {
        "closeButton": true,
        "positionClass": "toast-bottom-right",
        "progressBar": true
    }

    $("#ddlPaymentMode").select2();

    //datepicker plugin
    $("#txtChequeDate").datepicker({
        autoclose: true
    });

    //iCheck for checkbox and radio inputs
    $('input[type="checkbox"].minimal').iCheck({
        checkboxClass: 'icheckbox_square-blue',       
        increaseArea: '20%'
    });

    $(document).on("change", "#ddlPaymentMode", function () {
        var currPaymentMode = $(this).val();
        //CASH PAYMENT
        if (currPaymentMode == 2) {
            $(".chequePymnt").slideDown();
        }
        else {
            $(".chequePymnt").slideUp();
        }
        return false;
    });

    //hinding all paynow button except the first unpaid row
    var DisablePayButtonExceptFirstUnpaid = function () {

        $tr = $("#tblPaymentDetails tbody tr");

        $($tr).not($('input[value=False]:first').closest('tr')).each(function () {
            $(this).find(".paynow").hide();
        });
    };

    //Checking first unpaid date with current date
    var Check_DueDate_EmailValidation = function () {

        var currDate = new Date();
        var day = currDate.getDate();
        var month = currDate.getMonth() + 1; //January is 0!
        var year = currDate.getFullYear();
        currDate = month + "/" + day + "/" + year;

        $tr = $("#tblPaymentDetails tbody tr");
        var $unpaidTR = $('input[value=False]:first').closest('tr');
        var unpaidDueDate = new Date($unpaidTR.find(".duedate").val());

        //if duedate is greater than unpaid date
        if (new Date(currDate) > new Date(unpaidDueDate)) {
            $unpaidTR.find(".paynow").hide();
            $unpaidTR.find(".error-dueDate-over").show();
        }
        else {

            $unpaidTR.find(".duedate").val(currDate);
        }
        //if (EmailValidation()) {

        //}
        //else {
        //    $tr = $("#tblPaymentDetails tbody tr");
        //    var $unpaidTR = $('input[value=False]:first').closest('tr');
        //    $unpaidTR.find(".paynow").hide();
        //    $unpaidTR.find(".error-emailValidation").show();
        //}




    }

    //User when clicks on paynow button
    $(document).on("click", ".paynow", function () {

        var form = $("#frmAdd");
        if (form.valid()) {
            var currAmount = $(this).closest('tr').find('.totalAmt').val();
            var studentName = $("#txtStudentName").val();
            var receiptDate = new Date($(this).closest('tr').find('.duedate').val());
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

                };
            });
        };
        return false;

    });



    //User when clicks on modal pay now button
    $(document).on("click", "#btn_Receipt_PayNow", function () {

        //show alert if confirm payment checkbox has not been checked
        var isPaymentConfirmed = $("#chkbxConfirmReceipt").prop("checked");
        if (isPaymentConfirmed) {
            $("#divModalReceiptDetails").modal('hide');

            $("#frmAdd").ajaxSubmit({
                dataType: "json",
                beforeSubmit: function () {
                    $.blockUI({ message: '<h3><img src="../plugins/jQueryBlockUI/images/busy.gif" /> <b>Please wait... </b></h3>' });
                },
                success: function (result) {

                    if (result.Status == "success") {
                        generateReceipts();
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
            //var $td = $tr.eq(errorIndex).find('.dueDate').closest('td');
            var $div = $("#chkbxConfirmReceipt").closest('.divConfirmation');
            $($div).append("<div class='payment-confirmation field-validation-error'>Please confirme the above </div>")
            setTimeout(function () {
                $('.payment-confirmation').fadeOut(1500);

            }, 3000);
        }

       

    });

    //Generate Receipt and send sms and email
    var generateReceipts = function () {
        var href = $("#frmAdd").data("receipt-generate-url");
        $.ajax({
            type: "GET",
            url: href,
            datatype: "json",
            success: function (data) {
                $.unblockUI();
                if (data.Status == "success") {
                    toastr.success("Successfully saved the details.");
                    setTimeout(function () {
                        var url = $(form).data("redirect-url");
                        window.location.href = url;
                    }, 2000);


                    var printpdf_download_url = $("#frmAdd").data("download-url");
                    printpdf_download_url = printpdf_download_url.replace('param2_placeholder', data.PdfName);
                    window.location.href = printpdf_download_url;

                }
                else {
                    toastr.warning("Successfully saved the Student Receipt.But error while sending email or sms");
                }
                //toastr.success("Successfully saved the details.")


            },
            error: function (err) {
                toastr.error("Error:" + this.message)
            }
        });
    }

    //Duplicate receipt click function
    $(document).on("click", ".dupReceipt", function () {

        var redirectUrl = $(this).attr("href");
        window.location.href = redirectUrl;

        //var currAmount = $(this).closest('tr').find('.totalAmt').val();

        //bootbox.confirm("Are you sure you want generate duplicate receipt of <b>Rs." + currAmount + "</b>", function (result) {
        //    if (result) {
        //        var receiptId = $(this).data("id");
        //        var href = $(this).attr("href");
        //        $.blockUI({ message: '<h3><img src="../plugins/jQueryBlockUI/images/busy.gif" /> <b>Please wait... </b></h3>' });
        //        $.ajax({
        //            type: "GET",
        //            url: href,
        //            data: { receiptId: receiptId },
        //            datatype: "json",
        //            success: function (data) {
        //                $.unblockUI();
        //                if (data == "success") {
        //                    toastr.success("Successfully send the receipt details send to students emailId.")
        //                }
        //                else {
        //                    toastr.warning("Error while sending email");
        //                }

        //            },
        //            error: function (data) {
        //                $.unblockUI();
        //                toastr.error("Error sending email to student");
        //            }
        //        });
        //    }

        //});



        //return false;

    });

    //if paid count is greater than one and email is not validated return false
    var EmailValidation = function () {
        var emailValidated = true;
        $tr = $("#tblPaymentDetails tbody tr");
        var paidCount = $('input[value=True]').closest('tr').length;
        var isEmailValidated = $("#hFieldEmailValidation").val();
        if (paidCount >= 1) {
            if (isEmailValidated == "NotVerified") {
                emailValidated = false;
            };
        }

        return emailValidated;
    };

    //Gets the callout message of payment details
    var GetCallOutMessage = function () {
        var totalFee = "TotalFee - &#8377; " + $("#txtTotalCourseFee").val();
        var totalST = "TotalST - &#8377; " + $("#txtSTAmount").val();
        var totalAmt = "TotalAmount - &#8377; " + $("#txtTotalAmount").val();
        $("#titleTotalamt").html(totalFee + ' ' + ' ' + totalST + ' ' + totalAmt);
    };

    $(document).on("click", ".btnEditRegn", function () {
        var url = $(this).data("regn-url");
        window.location.href = url;
        return false;

    });

    //disable paymenttype if paidcount is equal to row length
    var PaymentTypeValidation = function () {
        var $trCount = $("#tblPaymentDetails tbody tr").length;
        var paidCount = $('input[value=True]').closest('tr').length;
        if ($trCount == paidCount) {
            $("#ddlPaymentMode").prop("disabled", true);

        }
        else {
            $("#ddlPaymentMode").prop("disabled", false);
        }

    }


    $(document).on("click", ".btnPrint", function () {
        var srcAttr = $(this).attr("href");
        var iFrame = $(this).parent().find(".clsiframePrint");
        var iFrameId = $(iFrame).attr('id');
        callPrint(iFrameId);
        return false;
        //var iFrame_Print = $('#iframePrint');
        //srcAttr = srcAttr + '?c=' + new Date().getTime();//force new URL
        //$(iframePrint).attr('src', srcAttr);
        //setTimeout(function () {

        //    iFrame_Print.bind('load', function () { //binds the event
        //        callPrint();
        //    });


        //}, 2000);

        //var downloadUrl = $(this).data("download-url");
        //window.location.href = downloadUrl;

        //return false;

    });

    function callPrint(iFrameId) {

        var PDF = document.getElementById(iFrameId);
        PDF.focus();
        PDF.contentWindow.print();
       
    }






});