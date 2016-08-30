$(function () {
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


    $("#ddlCourseMultiple").on("select2:select select2:unselect", function (e) {

        var data = e.params.data.id;       
        if (data != "") {
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

});