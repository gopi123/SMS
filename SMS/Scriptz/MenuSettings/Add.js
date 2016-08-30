$(document).ready(function () {


    //Select2 for dropdownlist
    $("#ddlRoles").select2({
        placeholder: "Select Department",
        allowClear: true
    });

    //Select2 for dropdownlist
    $("#ddlDesignation").select2({
        placeholder: "Select Designation",
        allowClear: true
    });

    //iCheck for checkbox and radio inputs
    $('input[type="checkbox"].minimal').iCheck({
        checkboxClass: 'icheckbox_square-blue',
        increaseArea: '20%'
    });

    //toastr options
    toastr.options = {
        "closeButton": true,
        "positionClass": "toast-bottom-right"
    }


    //hide/show experience section based on radiobutton change
    $(document).on('ifChecked ifUnchecked', '.checkAll', function (event) {
        var checkBoxAll = $(this);
        var checkBoxes = $(checkBoxAll).parents('tr').find('input.single');
        if (event.type == 'ifChecked') {
            checkBoxes.iCheck('check');
        }
        else {
            checkBoxes.iCheck('uncheck');
        }

    });

    //check/uncheck parentcheckbox depending upon childcheckbox
    $(document).on('ifChanged', '.single', function (event) {
        var checkBoxes = $(this).parents('tr').find('input.single');;
        var checkBoxAll = $(this).parents('tr').find('input.checkAll');
        if (checkBoxes.filter(':checked').length == checkBoxes.length) {
            checkBoxAll.prop('checked', 'checked');
        } else {
            checkBoxAll.removeAttr('checked');
        }
        checkBoxAll.iCheck('update');

    });

    //clears all checkbox
    function clearCheckBox() {
        var checkAll = $('input.checkAll');//get parentcheckboxes
        var checkBoxes = $('input.single');//get childcheckboxes
        checkAll.iCheck('uncheck');
        checkBoxes.iCheck('uncheck');
    }

    //IsLoading function
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

    //show or hide validation on dropdown change
    $("#ddlRoles,#ddlDesignation").change(function (evt, params) {
        $(evt.target).valid();
    });



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
                        GetMenuList();
                        toastr.success("Successfully saved the details.")
                    }
                    else if (data.message == "warning") {
                        toastr.info("Details already exists")
                    }
                    else {
                        toastr.error("Error:Oops.. some thing gone wrong")
                    }
                },
                error: function (data) {
                    isLoading(false);
                    toastr.error("Exception:Oops.. some thing gone wrong");
                }
            });
        }

        return false;
    }

    var clearAll = function () {
        clearCheckBox();
        $("#ddlRoles").select2("val", "");//resetting dpdwndistrict for clearing the selected option
        return false;
    };

    var GetDesignation = function () {

        var roleId = $("#ddlRoles").val();
        var spinner = $(this).parent('div').parent('div').find('.spinner');
        var $ddlDesignation = $("#ddlDesignation");
        var href = $("#divDesignation").data("url");

        if (roleId != "") {

            $(spinner).toggle(true);

            $.ajax({
                type: "POST",
                url: href,
                data: { roleId : roleId },
                datatype: "json",
                success: function (data) {
                    $(spinner).toggle(false);
                    $ddlDesignation.html('');//clearing the dpdwn html
                    $ddlDesignation.select2("val", "");//resetting dpdwn for clearing the selected option
                    $ddlDesignation.append('<option></option>')
                    $.each(data, function () {
                        $ddlDesignation.append($('<option></option>').val(this.Id).html(this.Name));
                    });
                },
                error: function (err) {
                    $(spinner).toggle(false);
                    toastr.error("Error:" + this.message)
                }
            });
        }
        else {
            $ddlDesignation.html('');//clearing the dpdwn html
            $ddlDesignation.select2("val", "");//resetting dpdwn for clearing the selected option
        }

    }

    var GetMenuList = function () {

        var designationId = $("#ddlDesignation").val();
        var href = $("#divMenuList").data("url");

        $.ajax({
            type: "GET",
            url: href,
            data: { designationId: designationId },
            sucess: function (data) {

            },
            error: function (err) {
                toastr.error("Exception some thing happened" + err);
            }
        }).done(function (data) {
            var $target = $("#MenuView");
            $($target).replaceWith(data);
            //iCheck for checkbox and radio inputs
            $('input[type="checkbox"].minimal').iCheck({
                checkboxClass: 'icheckbox_square-blue',
                increaseArea: '20%'
            });
            //Checkbox update
            $("#tblMenuList tr").each(function (i, row) {
                var checkBoxes = $(row).find('input.single');;
                var checkBoxAll = $(row).find('input.checkAll');
                if (checkBoxes.filter(':checked').length == checkBoxes.length) {
                    checkBoxAll.prop('checked', 'checked');
                } else {
                    checkBoxAll.removeAttr('checked');
                }
                checkBoxAll.iCheck('update');
            })

        });
    }

    //insert event on submit  click 
    $(document).on("click", "#btnSubmit", ajaxInsert);

    //clear all on on cancel  click 
    $(document).on("click", "#btnCancel", clearAll);

    //Get Designation on Department Change
    $(document).on("change", "#ddlRoles", GetDesignation);

    $(document).on("change","#ddlDesignation",GetMenuList)



});