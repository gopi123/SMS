//WalInnReport.JS

$(function () {

    //select2 plugin
    $("#ddlFinYear").select2();
    $("#ddlCentre").select2();
    $("#ddlEmployee").select2();
    $("#ddlCategory").select2();



    var stDate;
    var endDate;

    function calculateDate() {
        stDate = $("#txtFrom").datepicker({
            autoclose: true,
            startDate: getStartDate(),
            endDate: getEndDate()

        })

        edDate = $("#txtTo").datepicker({
            autoclose: true,
            startDate: getStartDate(),
            endDate: getEndDate()
        })


    }

    function getStartDate() {
        var startYear = $('#ddlFinYear').val().split('-');
        var startDate = new Date();
        startDate.setFullYear(startYear[0], 3, 1);
        return startDate;
    }

    function getEndDate() {
        var endYear = $('#ddlFinYear').val().split('-');
        var endDate = new Date();
        endDate.setFullYear(endYear[1], 2, 31);
        return endDate;
    }

    //Remove blank spaces from select options
    $('select option').filter(function () {
        return !this.value || $.trim(this.value).length == 0 || $.trim(this.text).length == 0;
    }).remove();

    var GetEmployeeList = function () {

        var finYear = $("#ddlFinYear").val();
        var centreId = $("#ddlCentre").val();
        var categoryType = $("#ddlCategory").val();
        var currDpDwnId = $(this).attr("id");

        var href = $("#divCentre").data("href");
        var spinner = $(this).parent('div').find('.spinner');
        var $ddlEmployee = $("#ddlEmployee");
        var i = 0;
        if (centreId != "") {

            $(spinner).toggle(true);

            $.ajax({
                type: "GET",
                url: href,
                data: { centreId: centreId, categoryType: categoryType, financialYear: finYear },
                datatype: "json",
                success: function (data) {
                    $(spinner).toggle(false);
                    $ddlEmployee.html('');//clearing the dpdwn html
                    $ddlEmployee.select2("val", "");//resetting dpdwn for clearing the selected option
                    //$ddlEmployee.append('<option></option>')
                    $.each(data, function () {
                        i++;
                        $ddlEmployee.append($('<option></option>').val(this.Id).html(this.Name));
                    });

                    if (i > 1) {
                        $("#ddlEmployee").select2("val", "-1");
                    }
                    else {
                        $("#ddlEmployee").select2("val", $('#ddlEmployee option:eq(1)').val());
                    }

                    if (currDpDwnId == "ddlFinYear") {


                        $("#txtFrom").datepicker("remove");
                        $("#txtTo").datepicker("remove");

                        $("#txtFrom").datepicker('setDate', getStartDate());
                        $("#txtFrom").datepicker('setStartDate', getStartDate());
                        $("#txtTo").datepicker("setDate", getEndDate());
                        $("#txtTo").datepicker("setEndDate", getEndDate());



                    }

                },
                error: function (err) {
                    $(spinner).toggle(false);
                    toastr.error("Error:" + this.message)
                }
            });
        }
        else {
            $ddlEmployee.html('');//clearing the dpdwn html
            $ddlEmployee.select2("val", "");//resetting dpdwn for clearing the selected option
        }


    }


    $(document).on("change", "#ddlCentre", GetEmployeeList);
    $(document).on("change", "#ddlCategory", GetEmployeeList);
    $(document).on("change", "#ddlFinYear", GetEmployeeList);

    calculateDate();

    $(document).on("change", "#ddlEmployee", function (e) {
        $(e.target).valid();
    });


    $(document).on("click", "#btnSubmit", function () {
        var form = $(this).closest('form');
        var href = $(form).attr('action');
        if (form.valid()) {
            var formData = $(form).serialize();
            $.ajax({
                type: "POST",
                url: href,
                data: formData,
                datatype: "json",
                success: function (data) {
                    if (data.message == "success") {
                        var redirectUrl = $(form).data("redirect-url");
                        window.location.href = redirectUrl;
                    }
                    else if (data.message == "warning") {
                        toastr.info("Details already exists")
                    }
                    else if (data.message == "error") {
                        toastr.info("Error:Something gone wrong")
                    }
                    else {
                        toastr.error("Exception:Something gone wrong")
                    }
                },
                error: function (data) {
                    isLoading(false);
                    toastr.error("Exception:some thing gone wrong");
                }
            });
        }

        return false;

    });


});