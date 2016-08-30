//DISCOUNT SETTING => EDIT.js

$(function () {

    var form = $("#frmEdit");
    form.validate({
        errorPlacement: function errorPlacement(error, element) { element.before(error); }
    });
    form.find("#DiscountSettingsStepz").steps({
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

            if (currentIndex == 0) {
                GetDiscountSettings()
            }

            return true

        },
        onFinishing: function (event, currentIndex) {

            return ValidateDiscountSetting();;
        },
        onFinished: function (event, currentIndex) {
            $(this).steps("previous");
            ajaxUpdate();

        }
    });

    $("#ddlRoleList").select2({
        placeholder: "Select Role",
        allowClear: true
    });

    $("#ddlGroupList").select2({
        placeholder: "Select Group "

    });

    //toastr options
    toastr.options = {
        "closeButton": true,
        "positionClass": "toast-bottom-right",
        "progressBar": true
    }

    var GetDiscountSettings = function () {

        var groupName = $("#hFieldGroupName").val();
        var roleIds = $("#ddlRoleList").val();
        var href = $("#frmEdit").data("get-discountsetting");

        $.ajax({
            type: "GET",
            url: href,
            data: { groupName: groupName, roleIds: roleIds },
            datatype: "html",
            traditional: true,
            success: function (data) {
                $("#divDiscountSetting").html(data);
            },
            error: function (data) {
                toastr.error("Oops..Some thing gone wrong");
            }
        });
    }


    var ValidateDiscountSetting = function () {
        var counter = 0;

        var $tr = $("#divDiscountSetting tbody tr");

        for (var i = 0; i < $tr.length; i++) {

            var errorIndex = $($tr[i]).index();

            var foundation = parseInt($($tr[i]).find('.fndn').val());
            var diploma = parseInt($($tr[i]).find('.dpma').val());
            var professional = parseInt($($tr[i]).find('.pfsl').val());
            var md = parseInt($($tr[i]).find('.mrdpma').val());

            if (foundation > 100) {
                counter++;
                var $td = $tr.eq(errorIndex).find('.fndn').closest('td');
                $($td).append("<span class='fndn-mismatch field-validation-error'>Please enter value less than 100 </span>")
                setTimeout(function () {
                    $('.fndn-mismatch').fadeOut(1500);

                }, 3000);
            }

            if (diploma > 100) {
                counter++;
                var $td = $tr.eq(errorIndex).find('.dpma').closest('td');
                $($td).append("<span class='dpma-mismatch field-validation-error'>Please enter value less than 100  </span>")
                setTimeout(function () {
                    $('.dpma-mismatch').fadeOut(1500);

                }, 3000);
            }

            if (professional > 100) {
                counter++;
                var $td = $tr.eq(errorIndex).find('.pfsl').closest('td');
                $($td).append("<span class='pfsl-mismatch field-validation-error'>Please enter value less than 100  </span>")
                setTimeout(function () {
                    $('.pfsl-mismatch').fadeOut(1500);

                }, 3000);
            }


            if (md > 100) {
                counter++;
                var $td = $tr.eq(errorIndex).find('.mrdpma').closest('td');
                $($td).append("<span class='mrdpma-mismatch field-validation-error'>Please enter value less than 100  </span>")
                setTimeout(function () {
                    $('.mrdpma-mismatch').fadeOut(1500);

                }, 3000);
            }

        }

        if (counter > 0) {
            return false;
        }

        else {
            return true;
        }

    };


    var ajaxUpdate = function () {
        var form = $("#frmEdit");
        var href = $(form).data('edit-discountsetting');
        var redirectUrl = $(form).data('redirect-url');
        if (form.valid()) {
            $.blockUI({ message: '<h3><img src="../plugins/jQueryBlockUI/images/busy.gif" /> <b>Please wait... </b></h3>' });
            var formData = $(form).serialize();
            $.ajax({
                type: "POST",
                url: href,
                data: formData,
                datatype: "json",
                success: function (data) {
                    $.unblockUI();
                    if (data == "success") {
                        setTimeout(function () {
                            window.location.href = redirectUrl;
                        }, 3000);
                        toastr.success("Successfully saved the details.")
                    }
                    else {
                        toastr("Error:Oops.. some thing gone wrong")
                    }
                },
                error: function (data) {
                    $.unblockUI();
                    toastr("Exception:Oops.. some thing gone wrong");
                }
            });
        }
        return false;
    }


});