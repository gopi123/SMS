//COURSEFULLEDIT ==> INDEX.JS
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
                { "data": "Status" },


            ],
            "aoColumnDefs": [

              {
                  "targets": [8],
                  "bSortable": false,
                  "render": function (data, type, row) {
                      var arr = data.split('_');
                      if (arr[0] == "validated") {
                          return '<span class="label label-info">Validated,' + arr[1] + '</span>'
                      }
                      else if (arr[0] == "approved") {
                          return '<span class="label label-success">Approved,' + arr[1] + '</span>'
                      }
                      else if (arr[0] == "rejected") {
                          return '<span class="label label-danger">Rejected,' + arr[1] + '</span>'
                      }
                      else {
                          return '<span class="label label-warning">Pending</span>'
                      }


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