//WALKINN ==> INDEX.JS
$(function () {
    var table;
    //Datatable function
    

    var GetDataTable = function () {

        var currFinYear = $("#ddlFinYearList").val();
        var currStudType = $("#ddlStudType").val();
        var currMonth = $("#ddlMonth").val();
        var url = $(".dTable").data("url");

        table = $(".dTable").dataTable({
            "bProcessing": true,//to show processing word
            "autoWidth": false,//to adjust width   
            "scrollX": true,
            "destroy": true,
            "aaSorting": [[8, 'desc']],
            "ajax": {
                "url": url,
                "method": "GET",
                "dataType": "json",
                "data": {
                    finYear: currFinYear,
                    studType: currStudType,
                    month:currMonth
                }
            },

            columns: [
                { "data": "WalkInnDate" },
                { "data": "Center" },
                { "data": "SalesPerson" },
                { "data": "Name" },
                { "data": "Mobile" },
                { "data": "CareGiverMobileNo" },
                { "data": "CourseRecommended" },
                { "data": "ExpJoinDate" },
                { "data": "Id" },
                { "data": "Email" }

            ],

            //Defining checkbox in columns
            "aoColumnDefs": [               
                {
                    "targets": [7],
                    "render": function (data, type, full, meta) {
                        if (data != null) {
                            var date = new Date(parseInt(data.substr(6)));
                            var month = date.getMonth() + 1;
                            return date.getDate() + '/' + month + '/' + date.getFullYear()
                        }
                        else {
                            return "NOT JOINED";
                        }

                    }
                },
                {
                    "targets": [8],
                    "bSortable": false,
                    "render": function (data, type, row) {
                        if (row.Status == "WALKINN") {
                            return '<div >' +
                                    '<div class="pull-left">' +
                                        '<a class="btn btn-xs btn-info editData pull-left"  data-id=' + data + ' >' +
                                        '<i class="fa fa-pencil"></i> Edit</a>' +
                                    '</div>' +
                                    '<div class="pull-left divRegister">' +
                                        '<a class="btn btn-info btn-xs  registerData pull-left"  data-id=' + data + ' data-name="' + row.Name + '" data-email="' + row.Email + '" data-mobile="' + row.Mobile + '">' +
                                        '<i class="fa fa-plus"></i>' +
                                        '  Register</a>' +
                                    '</div>' +
                                '</div>'
                        }
                        else {
                            return '<div >' +
                                    '<div class="col-sm-4 pull-left">' +
                                        '<a class="btn btn-xs btn-info editData pull-left"  data-id=' + data + ' >' +
                                        '<i class="fa fa-pencil"></i> Edit</a>' +
                                    '</div>' +
                                '</div>'
                        }
                    }
                },
                {
                    "targets": [9],
                    "visible": false,
                }

            ],

            //serialNo
            "fnRowCallback": function (nRow, aData, iDisplayIndex) {
                //var oSettings = table.fnSettings();
                //$("td:first", nRow).html(oSettings._iDisplayStart + iDisplayIndex + 1);
                //return nRow;
            },

            "fnDrawCallback": function () {
                $('.editData').click(function () {
                    var id = $(this).data('id');
                    var url = $("#frmIndex").data("redirect-url");
                    url = url.replace('param1_placeholder', id);
                    window.location.href = url;
                });


            }

        });
    };


    GetDataTable();

   


    $(document).on("click", ".registerData", function () {

        var studName = $(this).data("name");
        var studEmail = $(this).data("email");
        var studMobile = $(this).data("mobile");
        var studId = $(this).data("id");
        $("#mdlStudentName").val(studName);
        $("#mdlStudentEmail").val(studEmail);
        $("#mdlStudentPhoneNo").val(studMobile);
        $("#mdlStudentId").val(studId);
        $("#divModalEdit").modal({
            backdrop: 'static'
        });
        return false;
      
    });
   
    $(document).on("click", "#btnContinue", function () {
        
        var id = $("#mdlStudentId").val();
        var url = $("#frmIndex").data("register-url");
        url = url.replace('param1_placeholder', id);
        window.location.href = url
    });


    $(document).on("change", "#ddlFinYearList", function () {
        $('.dTable').dataTable().fnDestroy();
        GetDataTable();
    });

    $(document).on("change", "#ddlStudType", function () {
        $('.dTable').dataTable().fnDestroy();
        GetDataTable();
    });

    $(document).on("change", "#ddlMonth", function () {
        $('.dTable').dataTable().fnDestroy();
        GetDataTable();
    });


});