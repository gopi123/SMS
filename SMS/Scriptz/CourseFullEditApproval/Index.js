//COURSEFULLEDITAPPROVAL ==> INDEX.JS
$(function () {
    var table;
    //Datatable function


    var GetDataTable = function () {

        var currFinYear = $("#ddlFinYearList").val();
        var currMonth = $("#ddlMonth").val();
        var url = $(".dTable").data("url");

        table = $(".dTable").dataTable({
            "bProcessing": true,//to show processing word
            "autoWidth": false,//to adjust width   
            "scrollX": true,
            "destroy": true,

            "ajax": {
                "url": url,
                "method": "GET",
                "dataType": "json",
                "data": {
                    finYear: currFinYear,
                    month: currMonth
                }
            },

            columns: [
                { "data": "ReqDate" },
                { "data": "ReqBy" },
                { "data": "RegNo" },
                { "data": "StudentName" },
                { "data": "ExistingSoftwareUsed" },
                { "data": "NewSoftwareUsed" },
                { "data": "ExistingFee" },
                { "data": "NewFee" },
                { "data": "Id" },


            ],
            "aoColumnDefs": [

              
              {
                  "targets": [8],
                  "bSortable": false,
                  "render": function (data, type, row) {
                      return '<div >' +
                                  '<div class="pull-left">' +
                                      '<a class="btn btn-xs btn-info editData pull-left"  data-id=' + data + ' >' +
                                      ' View Details</a>' +
                                  '</div>' 
                                  
                              '</div>'


                  }
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