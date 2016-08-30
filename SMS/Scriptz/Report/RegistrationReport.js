
//REGISTRATIONREPORT.JS

$(function () {
    //select2 plugin
    $("#ddlFinYear").select2();
    $("#ddlCentre").select2();
    $("#ddlEmployee").select2();
    $("#ddlCourseCategory").select2(); 
    $("#ddlCourse").select2();

    var stDate;
    var endDate;

    function calculateDate() {
        stDate = $("#txtFrom").datepicker({
            autoclose: true,
            startDate: getStartDate(),
            endDate: getEndDate()
            //format: 'dd/mm/yyyy'
        }).on('changeDate', function (e) {
            $(e.target).valid();
            $("#txtTo").val('');
            $("#txtTo").datepicker('setStartDate', e.date);
        });

        endDate = $("#txtTo").datepicker({
            autoclose: true,
            startDate: getStartDate(),
            endDate: getEndDate()
        }).on('changeDate', function (e) {
            $(e.target).valid();

        });


    }

    function getStartDate() {
        var f = $('#ddlFinYear').val().split('-');
        var d = new Date();
        d.setFullYear(f[0], 3, 1);
        return d;
    }

    function getEndDate() {
        var f = $('#ddlFinYear').val().split('-');
        var d = new Date();
        d.setFullYear(f[1], 2, 31);
        return d;
    }

    //Remove blank spaces from select options
    $('select option').filter(function () {
        return !this.value || $.trim(this.value).length == 0 || $.trim(this.text).length == 0;
    }).remove();

    calculateDate();

    var GetEmployeeList = function () {
        var finYear = $("#ddlFinYear").val();
        var centreId = $("#ddlCentre").val();        
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
                data: { centreId: centreId, financialYear: finYear },
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

    var GetCourseList = function () {
        var categoryId = $("#ddlCourseCategory").val();
        var href = $("#divCourse").data("href");
        var spinner = $(this).parent('div').find('.spinner');
        var $ddlCourse = $("#ddlCourse");
        var i = 0;
        if (categoryId != "") {

            $(spinner).toggle(true);

            $.ajax({
                type: "GET",
                url: href,
                data: { categoryId: categoryId },
                datatype: "json",
                success: function (data) {
                    $(spinner).toggle(false);
                    $ddlCourse.html('');//clearing the dpdwn html
                    $ddlCourse.select2("val", "");//resetting dpdwn for clearing the selected option
                    //$ddlEmployee.append('<option></option>')
                    $.each(data, function () {
                        i++;
                        $ddlCourse.append($('<option></option>').val(this.Id).html(this.Name));
                    });

                    if (i > 1) {
                        $("#ddlCourse").select2("val", "-1");
                    }
                    else {
                        $("#ddlCourse").select2("val", $('#ddlCourse option:eq(1)').val());
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
    $(document).on("change", "#ddlFinYear", GetEmployeeList);
    $(document).on("change", "#ddlCourseCategory", GetCourseList);
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
                    else {
                        toastr.error("Error:Something gone wrong")
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