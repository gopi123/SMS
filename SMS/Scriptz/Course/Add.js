
// COURSE:Add.js
$(document).ready(function () {

    //text-input to uppercase
    $(".form-control").addClass('capitalise');

    //toastr options
    toastr.options = {
        "closeButton": true,
        "positionClass": "toast-bottom-right",
        "progressBar": true
    }
        

    //calling validation function of textbox change
    $('#txtInstallmentFee').change(function (evt, params) {
        $(evt.target).valid();
    });  
    


    //Calculating installment fee based on single fee
    $(document).on("keyup", "#txtSingleFee", function () {
        var singleFee = $(this).val();
        singleFee = Math.round(singleFee);       
        var installmentAmount = Math.round((singleFee * 10) / 100);
        var installmentFee = Math.round(singleFee + installmentAmount);        
        $("#txtInstallmentFee").val(installmentFee).change();
        
    });

    //isLoading function
    function isLoading(loading) {
        if (loading) {
            $('.content').loading({
                message: "Please wait ...",
                theme: 'dark'
            });
        }
        else {
            $('.content').loading('stop');
        }
    }

    //ajax insert
    var ajaxInsert = function () {
        var form = $(this).closest('form');
        var href = $(form).data('url');
        if (form.valid()) {
            isLoading(true);
            var formData = $(form).serialize();
            $.ajax({
                type: "POST",
                url: href,
                data: formData,
                datatype: "json",
                success: function (data) {
                    isLoading(false);
                    if (data.message == "success") {
                        clearAll();
                        toastr.success("Successfully saved the details.")
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

    //clear all controls
    var clearAll = function () {       
        $(".form-control").val('');        
        return false;
    };

    //insert event on submit  click 
    $(document).on("click", "#btnSubmit", ajaxInsert);

    //clear all on on cancel  click 
    $(document).on("click", "#btnCancel", clearAll);
});
