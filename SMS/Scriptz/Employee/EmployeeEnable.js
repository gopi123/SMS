//EMPLOYEE => EMPLOYEEENABLE.JS


$(function () {
    var table;
    //Datatable function 

    var GetDataTable = function () {
        var url = $(".dTable").data("url");

        table = $(".dTable").dataTable({
            "bProcessing": true,//to show processing word
            "autoWidth": false,//to adjust width   
            "scrollY": "700px",
            "scrollCollapse": true,
            "destroy": true,
            "ajax": {
                "url": url,
                "method": "GET",
                "dataType": "json"
            },

            columns: [
                { "data": "SlNo" },
                { "data": "Name" },
                { "data": "CenterCode" },
                { "data": "Designation" },
                { "data": "EmailId" },
                { "data": "Mobile" },
                { "data": "Id" },

            ],

            //Defining checkbox in columns
            "aoColumnDefs": [
                {
                    "targets": [0],
                    "bSortable": false
                },
                {
                    "targets": [6],
                    "bSortable": false,
                    "render": function (data, type, row) {
                        return '<div >' +
                                    '<div class="col-sm-4 pull-left">' +
                                        '<a class="btn btn-info updateData pull-left"  data-id=' + data + '  data-name="' + row.Name + '">' +
                                        '<i class="fa fa-check-square"></i> Enable</a>' +
                                    '</div>'
                        '</div>'
                    }
                }

            ],

            //serialNo
            "fnRowCallback": function (nRow, aData, iDisplayIndex) {
                var oSettings = table.fnSettings();
                $("td:first", nRow).html(oSettings._iDisplayStart + iDisplayIndex + 1);
                return nRow;
            },


        });
    }
   


    ////////////Delete Function///////////////////////
    //toastr options
    toastr.options = {
        "closeButton": true,
        "positionClass": "toast-bottom-right",
        "progressBar": true
    }

    GetDataTable();

    var ajaxUpdate = function () {
        var empId = $(this).data("id");
        var empName = $(this).data("name");
        var href = $("#frmEmpEnable").data("employeenable-url");


        bootbox.confirm("Are you sure you want to enable <strong>" + empName + "</strong>?", function (result) {
            if (result) {

                $.blockUI({ message: '<h3><img src="../plugins/jQueryBlockUI/images/busy.gif" /> <b>Please wait... </b></h3>' });


                $.ajax({
                    type: "POST",
                    url: href,
                    data: JSON.stringify({ empId: empId }),
                    datatype: "json",
                    contentType: 'application/json; charset=utf-8',
                    success: function (data) {
                        $.unblockUI();
                        if (data.message == "success") {
                            $('.dTable').dataTable().fnDestroy();
                            GetDataTable();
                            toastr.success("Successfully updated the details.")
                        }
                        else if (data.message == "error") {
                            $('.dTable').dataTable().fnDestroy();
                            GetDataTable();
                            toastr.error("Error!!Something went wrong.")
                        }
                        else {
                            $('.dTable').dataTable().fnDestroy();
                            GetDataTable();
                            toastr.error("Exception!!Something went wrong.")
                        }
                    },
                    error: function (data) {
                        $.unblockUI();
                        $('.dTable').dataTable().fnDestroy();
                        GetDataTable();
                        toastr.error("Exception!!Something went wrong");
                    }
                });
            }
        });



        return false;
    }

    $(document).on("click", ".updateData", ajaxUpdate);


});