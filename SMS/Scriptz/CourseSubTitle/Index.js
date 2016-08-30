
//COURSESUBTITLE : Index.js 

$(document).ready(function () {
    var table;//declared for datatable

    //toastr options
    toastr.options = {
        "closeButton": true,
        "positionClass": "toast-bottom-right",
        "progressBar": true
    }

    //IsLoading function
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
            { "data": "TitleName" },
            { "data": "TypeName" },
            { "data": "SeriesName" },
            { "data": "Id" }

        ],

        //Defining checkbox in columns
        "aoColumnDefs": [
            {
                "targets": [0],
                "bSortable": false
            },
            {
                "targets": 4,
                "bSortable": false,
                "render": function (data, type, row) {
                    return '<div id="test">' +
                                '<div class="col-sm-4 pull-left">' +
                                    '<a class="btn btn-info editData pull-left"  data-id=' + data + ' >' +
                                    '<i class="fa fa-edit"></i></a>' +
                                '</div>' +
                                '<div class="col-sm-4">' +
                                    '<a class="btn btn-danger deleteData pull-left" data-id=' + data + ' data-name="' + row.TitleName + '" >' +
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
    });

    //////////////////////Edit////////////////////////////////////////////////////////

    //Function GET Edit
    var ajaxEditGet = function () {

        var spinner = $(this).parent().parent('div').find('.spinner');
        var href = $("#divModalEdit").data("url");
        var courseSubTitleId = $(this).data('id');

        spinner.toggle(true);

        $.ajax({
            type: "GET",
            url: href,
            data: { _courseSubTitleId: courseSubTitleId },
            success: function (data) {
                spinner.toggle(false);
                $(".modal-body").html(data);
                $(".modal").modal({
                    backdrop: 'static'
                });
            },
            error: function (data) {
                spinner.toggle(false);
                toastr.error("Error:Some thing gone wrong");
            }
        });

        return false;

    };

    //Function POST Edit
    var ajaxEditPost = function () {

        var form = $("#frmEdit");
        var href = $("#divModalEdit").data("url");
        var spinner = $(this).parent('div').find('.spinner');
        var formData = $(form).serialize();
        var buttons = $('button.btnEdit')//button save and close

        if (form.valid()) {

            spinner.toggle(true);//start spinner
            $(buttons).prop('disabled', true);//set buttons to disabled mode

            $.ajax({
                type: "POST",
                url: href,
                data: formData,
                datatype: "json",
                success: function (data) {
                    if (data.message == "success") {
                        $(buttons).prop('disabled', false);//set buttons to enabled mode
                        spinner.toggle(false);//stop spinner
                        $(".modal").modal("hide");//hides modal
                        //reloads datatable inorder to affect changes
                        table.api().ajax.reload();
                        toastr.success("Successfully updated the details.")
                    }
                    else if (data.message == "error") {
                        $(buttons).prop('disabled', false);//set buttons to enabled mode
                        spinner.toggle(false);//stop spinner
                        $(".modal").modal("hide");//hides modal
                        toastr.error("Error!!Something went wrong.")
                    }
                    else {
                        $(buttons).prop('disabled', false);//set buttons to enabled mode
                        spinner.toggle(false);//stop spinner
                        $(".modal").modal("hide");//hides modal
                        toastr.error("Exception!!Something went wrong.")
                    }
                },
                error: function (data) {
                    $(buttons).prop('disabled', false);//set buttons to enabled mode
                    spinner.toggle(false);//stop spinner
                    $(".modal").modal("hide");//hides modal
                    toastr.error("Exception!!Something went wrong");
                }
            });

            return false;
        }

    };


    $(document).on("click", ".editData", ajaxEditGet)

    $(document).on("click", "#btnEditSubmit", ajaxEditPost)


    ////////////////////////////Delete////////////////////////////////////////////////////////////
    var ajaxDelete = function (id) {


        var href = $("#frmIndex").data("delete-url");
        isLoading(true);

        $.ajax({
            type: "POST",
            url: href,
            data: JSON.stringify({ _courseSubTitleId: id }),
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

        return false;
    }

    $(document).on("click", ".deleteData", function () {
        var _subTitleId = $(this).data("id");
        var _subTitleName = $(this).data("name");

        bootbox.confirm("Are you sure you want to delete <strong>" + _subTitleName + "</strong>?", function (result) {
            if (result) {
                ajaxDelete(_subTitleId)
            }
        });

    });

});