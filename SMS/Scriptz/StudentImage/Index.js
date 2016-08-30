//STUDENT IMAGE => INDEX.JS

$(function () {

    var table;
    //Datatable function
    var GetDataTable = function () {
       
        var url = $(".dTable").data("url");
        var photoMode = $("#ddlPhotoModeList").val();
        var currFinYear = $("#ddlFinYearList").val();
        var currMonth = $("#ddlMonth").val();
        table = $(".dTable").dataTable({

            "bProcessing": true,//to show processing word
            "autoWidth": false,//to adjust width  
            "scrollX": true,
            "destroy": true,
            //"aaSorting": [[10, 'desc']],
            "ajax": {
                "url": url,
                "method": "GET",
                "dataType": "json",
                "data": function (d) {
                    d.photoMode = photoMode;
                    d.finYear = currFinYear;
                    d.month = currMonth;
                }
            },

            columns: [
                { "data": "Date" },
                { "data": "StudentRegNo" },
                { "data": "StudentName" },              
                { "data": "Status" },
                { "data": "RegId" }
            ],

            //Defining checkbox in columns
            "aoColumnDefs": [
                {
                    "targets": [3],
                    "render": function (data, type, row) {
                        if (data == "WAITING") {
                            return '<small class="label label-warning"><i class="fa fa-clock-o"></i> WAITING</small>'
                        }
                        else if (data == "URGENT") {
                            return '<small class="label label-danger"><i class="fa fa-clock-o"></i> URGENT</small>'
                        }
                        else {
                            return '<small class="label label-success"><i class="fa fa-clock-o"></i> VERIFIED</small>'
                        }

                    }
                },
                {
                    "targets": [4],
                    "bSortable": false,
                    "render": function (data, type, row) {
                        return  '<div>'+
                                    '<div class="col-sm-4 pull-left">' +
                                            '<a class="btn btn-xs btn-info editData pull-left"  data-id=' + data + ' >' +
                                            '<i class="fa fa-upload"></i>Upload Image</a>' +
                                     '</div>'+
                                 '</div>'                                         
                    }
                },

            ],
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

    $(document).on("change", "#ddlPhotoModeList", function () {
        
        if ($("#ddlPhotoModeList").val() == 1) {
            $("#divFinYear").slideUp();
            $("#divMonth").slideUp();
        }
        else {
            $("#divFinYear").slideDown();
            $("#divMonth").slideDown();
           
        }
        $('.dTable').dataTable().fnClearTable();
        $('.dTable').dataTable().fnDestroy();
        GetDataTable();
    });

    $(document).on("change", "#ddlFinYearList", function () {
        $('.dTable').dataTable().fnDestroy();
        GetDataTable();

    });

    $(document).on("change", "#ddlMonth", function () {
        $('.dTable').dataTable().fnDestroy();
        GetDataTable();
    });

})