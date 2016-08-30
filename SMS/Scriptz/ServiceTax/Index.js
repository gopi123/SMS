//SERVICETAX: INDEX+EDIT

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
        "paging": false,//set paging to false
        "sDom": '<"top"r>',//set processing text to top of the grid

        "ajax": {
            "url": url,
            "method": "GET",
            "dataType": "json"
        },

        columns: [
            { "data": "" },
            { "data": "Percentage" },
            { "data": "FromDate" },
            { "data": "Id" }
        ],

        //Defining checkbox in columns
        "aoColumnDefs": [
            {
                "targets": 0,
                "bSortable": false,
                "render": function (data, type, full, meta) {
                    return '<label>ServiceTax</label>'
                }
            },
            {
                "targets": 1,
                "bSortable": false
            },
            {
                "targets": 2,
                "bSortable": false,
                "render": function (data, type, full, meta) {
                    var date = new Date(parseInt(data.substr(6)));
                    var month = date.getMonth() + 1;
                    return month + '/' + date.getDate() + '/' + date.getFullYear()
                }
            },
            {
                "targets": 3,
                "bSortable": false,
                "render": function (data, type, row) {
                    return  '<div >' +
                                '<div class="col-sm-2 pull-left">' +
                                    '<a class="btn btn-info editData pull-left"  data-id=' + data + ' data-date="' + row.FromDate + '" >' +
                                    '<i class="fa fa-edit"></i></a>' +
                                '</div>' +
                                '<div class="col-sm-2">' +
                                    '<a class="btn btn-danger deleteData pull-left" data-id=' + data + ' data-date="' + row.FromDate + '" >' +
                                    '<i class="fa fa-close"></i></a>' +
                                '</div>' +
                               ' <div class="pull-right spinner pull-left"  style="display:none" >' +
                                    '<i class="fa fa-refresh fa-spin spin-small "></i>' +
                                '</div>' +
                            '</div>'
                }
            }

        ]
    });
   
   

    //////////////////////Edit////////////////////////////////

    var compareDates = function (jsonDate)
    {
       
        var currentDate = moment().format("YYYY-MM-DD");        
        var serviceTaxDate = moment(jsonDate).format("YYYY-MM-DD");
        if (serviceTaxDate >= currentDate) {
            return true;
        }
        else {
            return false
        } 

    }

    //Function GET Edit
    var ajaxEditGet = function () {       

        var serviceTaxId = $(this).data('id');
        var jsonDate = $(this).data("date");
        var href = $("#divModalEdit").data("url");
        var spinner = $(this).parent().parent('div').find('.spinner');       

        var result = compareDates(jsonDate);

        if (result) {

            spinner.toggle(true);

            $.ajax({
                type: "GET",
                url: href,
                data: { serviceTaxId: serviceTaxId },
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
        }
        else {
            bootbox.alert("Cannot edit as <strong>ServiceTax date</strong> is less than <strong>Current date</strong>");
        } 

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
                        spinner.toggle(false);//stop spinner
                        $(".modal").modal("hide");//hides modal
                        toastr.error("Error!!Something went wrong.")
                    }
                    else {
                        spinner.toggle(false);//stop spinner
                        $(".modal").modal("hide");//hides modal
                        toastr.error("Exception!!Something went wrong.")
                    }
                },
                error: function (data) {
                    spinner.toggle(false);//stop spinner
                    $(".modal").modal("hide");//hides modal
                    toastr.error("Exception!!Something went wrong");
                }
            });

        }

        
        return false;

    };


    $(document).on("click", ".editData",ajaxEditGet);

    $(document).on("click", "#btnEditSubmit", ajaxEditPost)


    //////////////////////Delete/////////////////////////////////////////////////

    var ajaxDelete = function (id) {

        

        var href = $("#frmIndex").data("delete-url");
        isLoading(true);

        $.ajax({
            type: "POST",
            url: href,
            data: JSON.stringify({ serviceTaxId: id }),
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


    };

    $(document).on("click", ".deleteData", function () {
        var serviceTaxId = $(this).data('id');
        var jsonDate = $(this).data("date");       
        var spinner = $(this).parent().parent('div').find('.spinner');
        var serviceTaxDate = moment(jsonDate).format("MM/DD/YYYY");

        var result = compareDates(jsonDate);

        if (result) {
            bootbox.confirm("Are you sure you want to delete  <strong>Service Tax on "+serviceTaxDate+"</strong>?", function (result) {
                if (result) {
                    ajaxDelete(serviceTaxId)
                }
            });
        }
        else {
            bootbox.alert("Cannot delete as <strong>ServiceTax date</strong> is less than <strong>Current date</strong>");
        }

    })

});