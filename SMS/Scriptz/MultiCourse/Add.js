
//Add.js==>Multicourse 

$(function () {

    //text-input to uppercase
    $(".form-control").addClass('capitalise');

    //Select2 for dropdownlist
    $("#ddlCourseType").select2({
        placeholder: "Select a Course Type",
        allowClear: true
    });

    $("#ddlCourseSeries").select2({
        placeholder: "Select a Course Series",
        allowClear: true
    });

    $("#ddlCourseTitle").select2({
        placeholder: "Select a Course Title",
        allowClear: true
    });

    $("#ddlCourseMultiple").select2({
        placeholder: "Select any of the Courses ",
        allowClear: true
    });

    //toastr options
    toastr.options = {
        "closeButton": true,
        "positionClass": "toast-bottom-right",
        "progressBar": true
    }

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

    //show or hide validation on dropdown change
    $(document).on("change", "#ddlCourseType,#ddlCourseSeries,#ddlCourseTitle,#ddlCourseMultiple", function (evt, params) {
        //null checking is provided to donot show alert when ddlcoursetitle is loaded 
        
        if ($(this).val() != "" && $(this).val() != null) {
            $(evt.target).valid();
        }
    })
   


    //fills the dpdwn
    var FillCourseTitle = function (typeId, seriesId) {

        var ddlCourseTitle = $("#ddlCourseTitle");//get course title dropdown       
        var href = $("#divCourseTitle").data("url");     

        if (typeId != "" && seriesId != "") {
            $.ajax({
                type: "GET",
                url: href,
                data: { courseTypeId: typeId, courseSeriesId: seriesId },
                datatype: "json",               
                success: function (data) {                    
                    ddlCourseTitle.html('');//clearing the dpdwn html
                    ddlCourseTitle.select2("val", "");//resetting dpdwn for clearing the selected option
                    ddlCourseTitle.append('<option></option>')
                    $.each(data, function () {                       
                        ddlCourseTitle.append($('<option></option>').val(this.Id).html(this.Name));
                    });
                },
                error: function (err) {
                    toastr.error("Error:" + this.message)                    
                }
            });
        }
    };

    //GetCourseCode

    var GetCourseCode = function (typeId, seriesId) {
        var txtCourseCode = $("#txtCourseCode");
        var href = $("#divCourseCode").data("url");

        $.ajax({
            type: "GET",
            url: href,
            data: { courseTypeId: typeId, courseSeriesId: seriesId },
            datatype: "json",
            success: function (data) {                
                txtCourseCode.val(data)
            },
            error: function (err) {
                toastr.error("Error:" + this.message)
            }
        });

    };

    //Get Course Duration
    var GetDuration = function (duration, type) {
        //Gets the current duration value
        var currentDuration = $("#txtCourseDuration").val();
        var newDuration = 0;

        //Checking the value of current duration
        if (currentDuration == "") {
            currentDuration = 0
        }
        else {
            currentDuration = parseInt(currentDuration);
        }
       
        //based on type calculation is performed
        if (type == "add") {         
            newDuration = currentDuration + parseInt(duration);           
        }        
        else {
            newDuration = currentDuration - parseInt(duration);
        }

        $("#txtCourseDuration").val(newDuration);
        
    }

    //Get Course Single Fee
    var GetSingleFee = function (singleFee, type) {
        //Gets the current singlefee value
        var currentSingleFee = $("#txtSingleFee").val();
        var newSingleFee = 0;

        //Checking the value of current singlefee
        if (currentSingleFee == "") {
            currentSingleFee = 0
        }
        else {
            currentSingleFee = parseInt(currentSingleFee);
        }

        //based on type calculation is performed
        if (type == "add") {
            newSingleFee = currentSingleFee + parseInt(singleFee);
        }
        else {
            newSingleFee = currentSingleFee - parseInt(singleFee);
        }

        $("#txtSingleFee").val(newSingleFee);

    }

    //Get Course InstallmentFee
    var GetInstallmentFee = function (installmentFee, type) {
        //Gets the current singlefee value
        var currentInstallmentFee = $("#txtInstallmentFee").val();
        var newInstallmentFee = 0;

        //Checking the value of current singlefee
        if (currentInstallmentFee == "") {
            currentInstallmentFee = 0
        }
        else {
            currentInstallmentFee = parseInt(currentInstallmentFee);
        }

        //based on type calculation is performed
        if (type == "add") {
            newInstallmentFee = currentInstallmentFee + parseInt(installmentFee);
        }
        else {
            newInstallmentFee = currentInstallmentFee - parseInt(installmentFee);
        }

        $("#txtInstallmentFee").val(newInstallmentFee);

    }

    //clear all controls
    var clearAll = function () {
        $(".form-control").val('');
        //resetting dpdwn for clearing the selected option
        $("#ddlCourseType").select2("val", "");
        $("#ddlCourseSeries").select2("val", "");
        $("#ddlCourseTitle").select2("val", "");
        $("#ddlCourseMultiple").select2("val", "");
        return false;
    };

    var checkCourseSeriesValidaiton = function () {
        var count = $('#ddlCourseMultiple option:selected').length;
        var series = $("#ddlCourseSeries").val();

        if (series == 1 && count != 1) {
            bootbox.alert("Only <strong>One</strong> course is allowed in <strong>Foundation series</strong>");
            return false;
        }

        if (series == 2) {
            if (count > 2) {
                bootbox.alert("Only <strong>Two</strong> courses are allowed in <strong>Diploma series</strong>");
                return false;
            }
            if (count < 2) {
                bootbox.alert("<strong>Two</strong> courses should be selected in <strong>Diploma series</strong>");
                return false;
            }

        }
        if (series == 3) {
            if (count > 3) {
                bootbox.alert("Only <strong>Three</strong> courses are allowed in <strong>Professional series</strong>");
                return false;
            }
            if (count < 3) {
                bootbox.alert("<strong>Three</strong> courses should be selected in <strong>Professional series</strong>");
                return false;
            }

        }
        if (series == 4) {
            if (count <= 3) {
                bootbox.alert("Minimum <strong>Four</strong> courses should be selected in <strong>MasterDiploma series</strong>");
                return false;
            }
           

        }
        return true;
    }

    //ajax insert
    var ajaxInsert = function () {

        if (checkCourseSeriesValidaiton()) {
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
                        else if (data.message == "warning") {
                            toastr.info("Course combination already exists.")
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
        }        

        return false;
    }

   



    $(document).on("change", ".courseList", function (e) {

        e.preventDefault();
        var courseTypeId = $("#ddlCourseType").val();
        var courseSeriesId = $("#ddlCourseSeries").val();
        var spinner = $(this).parent().parent('div').find('.spinner');        

        if (courseTypeId != "" && courseSeriesId != "") {  
            spinner.toggle(true);            
            FillCourseTitle(courseTypeId, courseSeriesId);           
            GetCourseCode(courseTypeId, courseSeriesId);       
            spinner.toggle(false);
        }
        
        return false;

    })
  

    $("#ddlCourseMultiple").on("select2:select select2:unselect", function (e) {     
        
        
        var data = e.params.data.id;
        if (data != "")
        {
            var arr = data.split(",");
            var id = arr[0];
            var duration = arr[1];
            var singleFee = arr[2];
            var installmentFee = arr[3];

            if (e.type == "select2:select") {
                GetDuration(duration, "add");
                GetSingleFee(singleFee, "add");
                GetInstallmentFee(installmentFee, "add");
            }
            else {
                GetDuration(duration, "subtract");
                GetSingleFee(singleFee, "subtract");
                GetInstallmentFee(installmentFee, "subtract");
            }

            return false;
        }
        
    });

    //insert event on submit  click 
    $(document).on("click", "#btnSubmit", ajaxInsert);

    //clear all on on cancel  click 
    $(document).on("click", "#btnCancel", clearAll);


    

    
});