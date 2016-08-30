//EMPLOYEE ==> INDEX.JS
$(function () {
    var table;
    //Datatable function
    var url = $(".dTable").data("url");
    table = $(".dTable").dataTable({
        "bProcessing": true,//to show processing word
        "autoWidth": false,//to adjust width   
        "scrollY": "700px",
        "scrollCollapse": true,

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
                                    '<a class="btn btn-info editData pull-left"  data-id=' + data + ' >' +
                                    '<i class="fa fa-edit"></i></a>' +
                                '</div>' +
                                '<div class="col-sm-4">' +
                                    '<a class="btn btn-danger deleteData pull-left" data-id=' + data + ' data-name="' + row.Name + '" >' +
                                    '<i class="fa fa-close"></i></a>' +
                                '</div>' +
                               ' <div class="pull-right spinner col-sm-4"  style="display:none" >' +
                                    '<i class="fa fa-refresh fa-spin spin-small "></i>' +
                                '</div>' +
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

        "fnDrawCallback": function () {
            $('.editData').click(function () {
                var id = $(this).data('id');
                var url = $("#frmIndex").data("redirect-url");
                url = url.replace('param1_placeholder', id);
                window.location.href = url;
            });


        }
    });


    ////////////Delete Function///////////////////////
    //toastr options
    toastr.options = {
        "closeButton": true,
        "positionClass": "toast-bottom-right",
        "progressBar": true
    }

    //isLoading function
    var isLoading = function (loading) {
        if (loading) {
            $('.content').loading({
                message: "Please wait ...",
                theme: 'dark'
            });
        }
        else {
            $('.content').loading('stop');
        }
    };


    var ajaxDelete = function () {
        var empId = $(this).data("id");
        var empName = $(this).data("name");

        bootbox.confirm("Are you sure you want to delete <strong>" + empName + "</strong>?", function (result) {
            if (result) {

                var href = $("#frmIndex").data("delete-url");
                isLoading(true);


                $.ajax({
                    type: "POST",
                    url: href,
                    data: JSON.stringify({ employeeId: empId }),
                    datatype: "json",
                    contentType: 'application/json; charset=utf-8',
                    success: function (data) {
                        isLoading(false);
                        if (data.message == "success") {
                            table.api().ajax.reload();
                            toastr.success("Successfully deleted the details.")
                        }
                        else if (data.message == "error") {
                            toastr.error("Error!!Something went wrong.")
                        }
                        else {
                            toastr.error("Exception!!Something went wrong.")
                        }
                    },
                    error: function (data) {
                        isLoading(false);
                        toastr.error("Exception!!Something went wrong");
                    }
                });
            }
        });

       

        return false;
    }

    $(document).on("click", ".deleteData", ajaxDelete);


});