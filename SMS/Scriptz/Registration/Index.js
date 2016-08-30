//REGISTRATION ==> INDEX.JS
$(function () {
    var table;
    //Datatable function
    var GetDataTable = function () {
        var url = $(".dTable").data("url");
        var currFinYear = $("#ddlFinYearList").val();
        var currMonth = $("#ddlMonth").val();

        table = $(".dTable").dataTable({
            
            "bProcessing": true,//to show processing word
            "autoWidth": false,//to adjust width  
            "scrollX": true,
            "destroy":true,
            "aaSorting": [[10, 'desc']],
            "width": "20%",
            "ajax": {
                "url": url,
                "method": "GET",
                "dataType": "json",
                "data": function(d){
                    d.finYear = currFinYear;
                    d.month = currMonth;
                }
            },

            columns: [
                { "data": "RegDate" },
                { "data": "Centre" },
                { "data": "SalesPerson" },
                { "data": "StudentName" },
                { "data": "MobileNo" },
                { "data": "SoftwareUsed" },
                { "data": "Discount" },
                { "data": "CourseFee" },
                { "data": "NextDueDate" },
                { "data": "NextDueAmount" },
                { "data": "RegistrationID" }

            ],

            //Defining checkbox in columns
            "aoColumnDefs": [
               
                {
                    "targets": [8],
                    "bSortable": false,
                    "render": function (data, type, row) {
                        if (row.NextDueAmount != 0) {                          
                            return data + ',' + row.NextDueAmount
                        }
                        else {
                            return "FULL PAID"
                        }


                    }
                },
                {
                    "targets": [9],
                    "visible": false,
                },
                {
                    "targets": [10],
                    "bSortable": false,
                    "render": function (data, type, row) {

                        if (row.NextDueAmount != 0) {
                            return '<div >' +
                                   '<div class="pull-left">' +
                                       '<a class="btn btn-xs btn-info editData pull-left"  data-id=' + data + ' >' +
                                       '<i class="fa fa-pencil"></i> Edit</a>' +
                                   '</div>' +
                                   '<div class=" pull-left">' +
                                       '<a class="btn btn-xs btn-info receipt pull-left"  data-id=' + data + ' >' +
                                       '<i class="fa fa-rupee "></i> Receipt</a>' +
                                   '</div>' +
                               '</div>'
                        }
                        else {
                            return '<div >' +
                                   '<div class="pull-left">' +
                                       '<a class="btn btn-xs btn-info editData pull-left"  data-id=' + data + ' >' +
                                       '<i class="fa fa-pencil"></i> Edit</a>' +
                                   '</div>' +                                   
                               '</div>'
                        }
                       
                              
                    }
                }

            ],
            "fnDrawCallback": function () {
                $('.editData').click(function () {
                    var id = $(this).data('id');
                    var url = $("#frmIndex").data("redirect-url");
                    url = url.replace('param1_placeholder', id);
                    window.location.href = url;
                });
                $('.receipt').click(function () {
                    var id = $(this).data('id');
                    var url = $("#frmIndex").data("receipt-url");
                    url = url.replace('param1_placeholder', id);
                    window.location.href = url;
                });
            }
        });

    };

    GetDataTable();

    $(document).on("change", "#ddlFinYearList", function () {
        $('.dTable').dataTable().fnDestroy();
        GetDataTable();    
        
    });

    $(document).on("change", "#ddlMonth", function () {
        $('.dTable').dataTable().fnDestroy();
        GetDataTable();
    });
   



});