
//Discount Setting => Add.js


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
            ajaxInsert();

        }
    });

    $("#ddlRoleList").select2({
        placeholder: "Select Role",
        allowClear: true
    });

    $("#ddlGroupList").select2({
        placeholder: "Select Group ",
        allowClear: true
    });


    $("#ddlCentreList").select2({
        placeholder: "No group selected"
    });

    $('.fromDate').datepicker({
        autoclose: true,
        startDate: '+0d', // set default to today's date          
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


    var GetDiscountSettings = function () {
        var roleIds = [];
        roleIds = $("#ddlRoleList").val();
        var href = $("#frmAdd").data("get-discountsetting");

        $.ajax({
            type: "GET",
            url: href,
            data: { roleIds: roleIds },
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


    var ajaxInsert = function () {
        var form = $("#frmAdd");
        var href = $(form).data('add-discountsetting');
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
                        //GetDiscountSetting();
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

    var GetCentreCodeList = function (groupName, dpdwnValues,selectType) {

        var href = $("#divCentreCode").data('get-centrecode');
        var $ddlCentreList = $("#ddlCentreList");

        $.ajax({
            type: "GET",
            url: href,
            data: { groupName: groupName },
            traditional: true,
            datatype: "json",
            success: function (data) {
                if (selectType == "remove") {
                    $ddlCentreList.html('');//clearing the dpdwn html
                    $ddlCentreList.select2("val", "");//resetting dpdwn for clearing the selected option
                    $ddlCentreList.append('<option></option>')
                }

                $.each(data, function () {
                    $ddlCentreList.append($('<option></option>').val(this.Id).html(this.Name));
                    dpdwnValues.push(this.Id);
                });

                $ddlCentreList.select2("val", dpdwnValues);


            },
            error: function (data) {
                toastr("Exception:Oops.. some thing gone wrong");
            }
        });
        return false;
    };


    //ddlMulticourse select/unselect is called here
    $("#ddlGroupList").on("select2:select select2:unselect", function (e) {

        var centreCodevalues = [];
        var groupName = [];

        if (e.type == "select2:select") {

            groupName = e.params.data.id;

            // Loop through all the options in the select
            $("#divCentreCode option").each(function () {
                // log the value and text of each option
                centreCodevalues.push($(this).val())
                console.log($(this).val());
                console.log($(this).text());
            });

            GetCentreCodeList(groupName, centreCodevalues, "add");
        }
        else {
            groupName = $("#ddlGroupList").val();

            GetCentreCodeList(groupName, centreCodevalues, "remove");

        }

        

        //var courseId = $(this).val();
        ////For GetCourseList calculation
        //$("#txtMultiCourseId").val(courseId);

        ////Gets the last selected item
        //var lastSelectedCourseId = e.params.data.id;

        //var feeMode = $("input[name='InstallmentType']:checked").val();
        //if (e.type == "select2:select") {

        //    GetCourseDetails(lastSelectedCourseId, feeMode, "add");
        //}
        //else {

        //    GetCourseDetails(lastSelectedCourseId, feeMode, "subtract");

        //}


    });

});