//COURSE: Index.js

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
            { "data": "UploadedDate" },
            { "data": "StudentID" },
            { "data": "StudentName" },
            { "data": "Course" },
            { "data": "Id" }

        ],

        //Defining checkbox in columns
        "aoColumnDefs": [
            {
                "targets": [0],
                "bSortable": false
            },
            {
                "targets": 2,
                "bSortable": false
            },
            {
                "targets": 5,
                "bSortable": false,
                "render": function (data, type, row) {
                    return '<div >' +
                                '<div class="col-sm-4 pull-left">' +
                                   '<a class="btn btn-primary pull-left btn-xs btn-flat btnVerify" data-feedbackid=' + data + ' ' +
                                    'style="margin-right: 5px;"><i class="fa   fa-check-square-o"></i> Verify </a>' +
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
            $('.btnVerify').click(function () {
                var id = $(this).data('feedbackid');
                var url = $("#frmPjtRptApproval").data("redirect-url");
                url = url.replace('param1_placeholder', id);
                window.location.href = url;            });
           
        }
    });





    //////////////////////Edit////////////////////////////////

    //Function GET Edit
    var ajaxEditGet = function () {
       
        var feedbackId = $(this).data('feedbackid');
        var href = $("#frmPjtRptApproval").data("redirect-url");      

        w

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


    ////////////////////////Delete/////////////////////////////////////////////////////////


    var ajaxDelete = function (id) {


        var href = $("#frmIndex").data("delete-url");
        isLoading(true);


        $.ajax({
            type: "POST",
            url: href,
            data: JSON.stringify({ courseId: id }),
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
        var courseId = $(this).data("id");
        var courseName = $(this).data("name");

        bootbox.confirm("Are you sure you want to delete <strong>" + courseName + "</strong>?", function (result) {
            if (result) {
                ajaxDelete(courseId)
            }
        });

    });

});



