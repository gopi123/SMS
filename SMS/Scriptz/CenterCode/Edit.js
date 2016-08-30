//CENTERCODE ==> Edit.js

$(function () {

   
    //text-input to uppercase
    $(".form-control").addClass('capitalise');

    //Select2 for dropdownlist
    $("#ddlState").select2({
        placeholder: "Select  State",
        allowClear: true
    });

    $("#ddlDistrict").select2({
        placeholder: "Select  District",
        allowClear: true
    });

    $("#ddlBranchType").select2({
        placeholder: "Select  Branch",
        allowClear: true
    });

    $("#ddlFirmType").select2({
        placeholder: "Select  Firm",
        allowClear: true
    });

    $("#ddlEmployee").select2({
        placeholder: "Select  Employee",
        allowClear: true
    });

    //input mask
    $("#txtPincode").inputmask("999999");

    //toastr options
    toastr.options = {
        "closeButton": true,
        "positionClass": "toast-bottom-right",
        "progressBar": true
    }

    //Checkbox switch function
    $(".chkbxSwitch").bootstrapSwitch({
        onSwitchChange: function (event, state) {           
            $("#txtSTRegnNo").valid();
            var id = $(this).attr("id");
            if (id == "chkbxST") {
                if (state) {
                    $("#txtSTRegnNo").prop('disabled', false);
                }
                else {
                    $("#txtSTRegnNo").prop('disabled', true);
                    $("#txtSTRegnNo").val("");
                }
            }
        }
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

    //Changing textbox status depending upon chkbxST
    var currState = $("#chkbxST").bootstrapSwitch('state');
    if (!currState) {
        $("#txtSTRegnNo").prop('disabled', true);
    }

    //stop showing validation if someitem is selected
    $("#ddlState,#ddlDistrict,#ddlBranchType,#ddlFirmType,#ddlEmployee").change(function (evt, params) {
        if ($(this).val() != "" && $(this).val() != null) {
            $(evt.target).valid();
        }
    });

    //Append a delete column depending of firm type
    var AddColumnDelete = function () {
        var rowCount = 0;
        var i = 0;
        var firmType = $("#ddlFirmType").val();

        if (firmType == "1") {
            rowCount = 1;
        }
        else {
            rowCount = 2;
        }
        
        
        $('#tblPartnerDetails tbody tr').each(function (i, el) {
            i++;
            //insert delete column only after mandatory rows
            if (i > rowCount) {
                $(this).find('td').eq(5).after('<td><div class="tools"><a class="btn rowDelete"><i class="fa  fa-trash-o " style="color:red"></i></a></div></td>');
            }
            
        });
    };



    //isLoading function
    var isLoading = function (loading) {
        if (loading) {
            $('.content').loading({
                message: "Please wait ...",
                theme: 'dark'
            });
        }
        else {
            $('.content').loading('stop');
        }
    };

    

    var GetDistrict = function (stateId, spinner) {
        var $ddlDistrict = $("#ddlDistrict");
        var href = $("#divDistrict").data("url");

        if (stateId != "") {

            $(spinner).toggle(true);

            $.ajax({
                type: "GET",
                url: href,
                data: { stateId: stateId },
                datatype: "json",
                success: function (data) {
                    $(spinner).toggle(false);
                    $ddlDistrict.html('');//clearing the dpdwn html
                    $ddlDistrict.select2("val", "");//resetting dpdwn for clearing the selected option
                    $ddlDistrict.append('<option></option>')
                    $.each(data, function () {
                        $ddlDistrict.append($('<option></option>').val(this.Id).html(this.Name));
                    });
                },
                error: function (err) {
                    $(spinner).toggle(false);
                    toastr.error("Error:" + this.message)
                }
            });
        }
        else {
            $ddlDistrict.html('');//clearing the dpdwn html
            $ddlDistrict.select2("val", "");//resetting dpdwn for clearing the selected option
        }

    }

    var AddRow = function (rows, type) {

        //For firsttime addition of datatable
        if (type == "new") {

            var $tbody = $("#tblPartnerDetails tbody");
            $($tbody).html('');

            for (var i = 0; i < rows; i++) {

                var slno = parseInt(i + 1);

                var $row = $('<tr/>');
                $row.append(' <td class="slno">' + slno + '</td>');

                $row.append(' <td><input name="CenterCodePartnerDetail[' + i + '].PartnerName" type="text" class="form-control capitalise valid PartnerName" data-val="true" data-val-required="PartnerName" placeholder="Name" />'
                               + '<span class="field-validation-valid spanPartnerName" data-valmsg-for="CenterCodePartnerDetail[' + i + '].PartnerName" data-valmsg-replace="true"></span> </td>');

                $row.append(' <td><input name="CenterCodePartnerDetail[' + i + '].ContactNumber" type="text" class="form-control valid ContactNumber numberOnly" data-val="true" data-val-required="ContactNumber" placeholder="ContactNo" />'
                               + '<span class="field-validation-valid spanContactNumber" data-valmsg-for="CenterCodePartnerDetail[' + i + '].ContactNumber" data-valmsg-replace="true"></span> </td>');

                $row.append(' <td><input name="CenterCodePartnerDetail[' + i + '].EmailId" type="text" class="form-control capitalise valid EmailId" data-val="true" data-val-required="EmailId" data-val-email="Invalid EmailId" placeholder="EmailId" />'
                                + '<span class="field-validation-valid spanEmailId" data-valmsg-for="CenterCodePartnerDetail[' + i + '].EmailId" data-valmsg-replace="true"></span> </td>');

                $row.append(' <td><input name="CenterCodePartnerDetail[' + i + '].AltContactNumber" type="text" class="form-control valid AltContactNumber numberOnly" data-val="true" data-val-required="AltContactNumber" placeholder="AltContactNo" />'
                                + '<span class="field-validation-valid spanAltContactNumber" data-valmsg-for="CenterCodePartnerDetail[' + i + '].AltContactNumber" data-valmsg-replace="true"></span> </td>');

                $row.append(' <td><input name="CenterCodePartnerDetail[' + i + '].AltEmailId" type="text" class="form-control capitalise valid AltEmailId"  data-val="true" data-val-required="AltContactNumber" data-val-email="Invalid EmailId" placeholder="AltEmailId" />'
                                + '<span class="field-validation-valid spanAltEmailId" data-valmsg-for="CenterCodePartnerDetail[' + i + '].AltEmailId" data-valmsg-replace="true"></span> </td>');
                $tbody.append($row);
            }

        }

            //On clicking addRow button
        else {

            var slno = parseInt(rows + 1);
            var i = parseInt(rows);

            var $row = $('<tr/>');
            $row.append(' <td class="slno">' + slno + '</td>');

            $row.append(' <td><input name="CenterCodePartnerDetail[' + i + '].PartnerName" type="text" class="form-control capitalise  PartnerName valid" data-val="true" data-val-required="PartnerName" placeholder="PartnerName"  />'
                               + '<span class="field-validation-valid spanPartnerName" data-valmsg-for="CenterCodePartnerDetail[' + i + '].PartnerName" data-valmsg-replace="true"></span> </td>');

            $row.append(' <td><input name="CenterCodePartnerDetail[' + i + '].ContactNumber" type="text" class="form-control  ContactNumber numberOnly" data-val="true" data-val-required="ContactNumber" placeholder="ContactNo" id="txtContactNumber" />'
                               + '<span class="field-validation-valid spanContactNumber" data-valmsg-for="CenterCodePartnerDetail[' + i + '].ContactNumber" data-valmsg-replace="true"></span> </td>');

            $row.append(' <td><input name="CenterCodePartnerDetail[' + i + '].EmailId" type="text" class="form-control capitalise  EmailId" data-val="true" data-val-required="EmailId" data-val-email="Invalid EmailId" placeholder="EmailId" id="txtEmailId" />'
                            + '<span class="field-validation-valid spanEmailId" data-valmsg-for="CenterCodePartnerDetail[' + i + '].EmailId" data-valmsg-replace="true"></span> </td>');

            $row.append(' <td><input name="CenterCodePartnerDetail[' + i + '].AltContactNumber" type="text" class="form-control  AltContactNumber numberOnly" data-val="true" data-val-required="AltContactNumber" placeholder="AltContactNo" id="txtAltContactNumber" />'
                            + '<span class="field-validation-valid spanAltContactNumber" data-valmsg-for="CenterCodePartnerDetail[' + i + '].AltContactNumber" data-valmsg-replace="true"></span> </td>');

            $row.append(' <td><input name="CenterCodePartnerDetail[' + i + '].AltEmailId" type="text" class="form-control capitalise AltEmailId"  data-val="true" data-val-required="AltContactNumber" data-val-email="Invalid EmailId" placeholder="AltEmailId" id="txtAltContactNumber" />'
                            + '<span class="field-validation-valid spanAltEmailId" data-valmsg-for="CenterCodePartnerDetail[' + i + '].AltEmailId" data-valmsg-replace="true"></span> </td>');

            $row.append('<td><div class="tools"><a class="btn rowDelete"><i class="fa  fa-trash-o " style="color:red"></i></a></div></td>');

            $("#tblPartnerDetails tbody tr:last").after($row);

        }



    };
   
    //Function POST Edit
    var ajaxEditPost = function () {

        
        var form = $("#frmEdit");
        $(form).data('validator', null);
        $.validator.unobtrusive.parse($('form'))
        var href = $(form).data("url"); 
        var redirectUrl=$(form).data("redirect-url");
        var formData = $(form).serialize();      
        
        if (form.valid()) {     
            isLoading(true);
            $.ajax({
                type: "POST",
                url: href,
                data: formData,
                datatype: "json",
                success: function (data) {
                  
                    if (data.message == "success") {                                    
                        toastr.success("Successfully updated the details.")
                        window.location.href = redirectUrl;
                    }
                    else if (data.message == "error") {                       
                        toastr.error("Error!!Something went wrong.")
                    }
                    else {                       
                        toastr.error("Exception!!Something went wrong.")
                    }
                },
                error: function (data) {
                    isLoading(false);
                    toastr.error("Exception!!Something went wrong");
                }
            });

            return false;
        }

    };
    //Adds delete column on page load to the datatable
    AddColumnDelete();
    //Get District on State Change
    $(document).on("change", "#ddlState", function () {

        var stateId = $("#ddlState").val();
        var spinner = $(this).parent().parent('div').find('.spinner');
        GetDistrict(stateId, spinner);

    });
    //Show partner details table 
    $(document).on("change", "#ddlFirmType", function () {
        if ($(this).val() != "") {
            var rowCount = 0;
            var firmType = $("#ddlFirmType").val();

            if (firmType == "1") {
                rowCount = 1;
            }
            else {
                rowCount = 2;
            }

            AddRow(rowCount, "new");
            $("#divPartnerDetails").slideDown();

        }
        else {
            $("#divPartnerDetails").slideUp();
        }


    });
    //Add New row to datatabe
    $(document).on("click", "#btnAddRow", function (e) {

        var rowCount = $('#tblPartnerDetails tbody tr').length;    
        AddRow(rowCount, "exist");
        return false;

    });
    //RowDelete
    $(document).on("click", ".rowDelete", function () {
        //removing the current row
        $(this).parent().parent().parent().remove();
        var slno = 0;
        $("#tblPartnerDetails tbody tr").each(function (i, el) {

            //Resetting serialno
            slno++;
            var $tdSlno = $(this).find('.slno');
            $tdSlno.html(slno);

            //Resetting names of all texboxes and span
            var $tdPartnerName = $(this).find('.PartnerName');
            var $tdContactNumber = $(this).find('.ContactNumber');
            var $tdEmailId = $(this).find('.EmailId');
            var $tdAltContactNumber = $(this).find('.AltContactNumber');
            var $tdAltEmailId = $(this).find('.AltEmailId');

            var $spanPartnerName = $(this).find('.spanPartnerName');
            var $spanContactNumber = $(this).find('.spanContactNumber');
            var $spanEmailId = $(this).find('.spanEmailId');
            var $spanAltContactNumber = $(this).find('.spanAltContactNumber');
            var $spanAltEmailId = $(this).find('.spanAltEmailId');


            $tdPartnerName.removeAttr('name');
            $tdContactNumber.removeAttr('name');
            $tdEmailId.removeAttr('name');
            $tdAltContactNumber.removeAttr('name');
            $tdAltEmailId.removeAttr('name');

            $spanPartnerName.removeAttr('data-valmsg-for');
            $spanContactNumber.removeAttr('data-valmsg-for');
            $spanEmailId.removeAttr('data-valmsg-for');
            $spanAltContactNumber.removeAttr('data-valmsg-for');
            $spanAltEmailId.removeAttr('data-valmsg-for');

            $tdPartnerName.attr('name', 'CenterCodePartnerDetail[' + i + '].PartnerName');
            $tdContactNumber.attr('name', 'CenterCodePartnerDetail[' + i + '].ContactNumber');
            $tdEmailId.attr('name', 'CenterCodePartnerDetail[' + i + '].EmailId');
            $tdAltContactNumber.attr('name', 'CenterCodePartnerDetail[' + i + '].AltContactNumber');
            $tdAltEmailId.attr('name', 'CenterCodePartnerDetail[' + i + '].AltEmailId');

            $spanPartnerName.attr('data-valmsg-for', 'CenterCodePartnerDetail[' + i + '].PartnerName');
            $spanContactNumber.attr('data-valmsg-for', 'CenterCodePartnerDetail[' + i + '].ContactNumber');
            $spanEmailId.attr('data-valmsg-for', 'CenterCodePartnerDetail[' + i + '].EmailId');
            $spanAltContactNumber.attr('data-valmsg-for', 'CenterCodePartnerDetail[' + i + '].AltContactNumber');
            $spanAltEmailId.attr('data-valmsg-for', 'CenterCodePartnerDetail[' + i + '].AltEmailId');



        });

        return false;

    });   
    //Ajax Edit Post
    $(document).on("click", "#btnEditSubmit", ajaxEditPost)



   
});