
//MENUSETTINGS/INDEX.js

$(document).ready(function () {

    var table;//declared for datatable

    //Select2 for dropdownlist
    $("#ddlRoles").select2({
        placeholder: "Select a Role",
        allowClear: true
    });

    //toastr options
    toastr.options = {
        "positionClass": "toast-bottom-right",
        "progressBar": true
    }

    //iCheck for checkbox and radio inputs
    function iCheck() {
        $('input[type="checkbox"].minimal').iCheck({
            checkboxClass: 'icheckbox_square-blue',
            increaseArea: '20%'

        });
    }

    //Prevents the checkbox from clicking
    $(document).on('ifChecked ifUnchecked', '.minimal', function (e) {
        $(e).preventDefault();
    });


    //Datatable function
    var url = $("#dataTableMenuRole").data("url");   
    table = $("#dataTableMenuRole").dataTable({
        "bProcessing": true,//to show processing word
        "autoWidth": false,//to adjust width  
        "paging": false,//set paging to false
        "sDom": '<"top"rf>',//set processing text,search to top of the grid

        "ajax": {
            "url": url,
            "method": "POST",
            "dataType": "json",
            "data": function (d) {
                d.roleId = $("#ddlRoles").val()
            }
        },
        columns: [
            { "data": "MenuName" },
            { "data": "CanAdd" },
            { "data": "CanEdit" },
            { "data": "CanDelete" },
            { "data": "CanView" },
            { "data": "MenuRoleId" }

        ],
        //Defining checkbox in columns
        "aoColumnDefs": [
            {
                "targets": 0,
                "bSortable": false
            },
            {
                "targets": 1,
                "bSortable": false,
                "mRender": function (data, type, full, meta) {
                    return '<input type="checkbox"  class="minimal" ' + (data ? 'checked' : '') + '/>'

                }
            },
            {
                "targets": 2,
                "bSortable": false,
                "mRender": function (data, type, full, meta) {
                    return '<input type="checkbox" class="minimal" ' + (data ? 'checked' : '') + '/>'
                }
            },
            {
                "targets": 3,
                "bSortable": false,
                "mRender": function (data, type, full, meta) {
                    return '<input type="checkbox" class="minimal" ' + (data ? 'checked' : '') + '/>'
                }
            },
            {
                "targets": 4,
                "bSortable": false,
                "mRender": function (data, type, full, meta) {
                    return '<input type="checkbox" class="minimal" ' + (data ? 'checked' : '') + '/>'
                }
            },
            {
                "targets": 5,
                "bSortable": false,
                "render": function (data, type, full, meta) {
                    return '<div class="col-sm-8"><a class="btn btn-info editRole" data-id=' + data + ' >' +
                           '<i class="fa fa-edit"></i></a>' +
                           '<div class="pull-right spinner"  data-id=' + data + '  style="display:none" >' +
                           '<i class="fa fa-refresh fa-spin spin-small "></i></div></div>'
                }

            }

        ],

        "fnDrawCallback": function () {
            iCheck();
        }

    });


    //calling datatable reload function on dropdown change
    $("#ddlRoles").change(function () {
        table.api().ajax.reload();
        return false;
    });


    /////////////////////////////////Edit//////////////////////////////////////////////

    //Function GET Edit
    var ajaxGet = function (e) {


        var spinner = $(this).parent('div').find('.spinner');
        var href = $("#editMenuSettings").data("url");
        var menuRoleId = $(this).data('id');

        spinner.toggle(true);

        $.ajax({
            type: "GET",
            url: href,
            data: { menuRoleId: menuRoleId },
            success: function (data) {
                spinner.toggle(false);
                $(".modal-body").html(data);
                $(".modal").modal({
                    backdrop: 'static'
                });
            },
            error: function (data) {
                spinner.toggle(false);
                toastr.error("Oops..Some thing gone wrong");
            }
        });

        return false;

    };

    //Function POST Edit
    var ajaxUpdate = function (e) {

        var form = $("#frmEditRoles");
        var href = $(form).data("url");
        var spinner = $(this).parent('div').find('.spinner');
        var formData = $(form).serialize();
        var buttons = $('button.btnEdit')//button save and close

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
                    toastr.error("Error..Something gone wrong.")
                }
                else {
                    $(buttons).prop('disabled', false);//set buttons to enabled mode
                    spinner.toggle(false);//stop spinner
                    $(".modal").modal("hide");//hides modal
                    toastr.error("Exception..Something gone wrong.")
                }
            },
            error: function (data) {
                spinner.toggle(false);//stop spinner
                $(".modal").modal("hide");//hides modal
                toastr.error("Oops..Some thing gone wrong");
            }
        });

        return false;
    }


    //ajaxGet on edit button click
    $(document).on('click', '.editRole', ajaxGet);

    //ajaxUpdate pn Save button click
    $(document).on("click", "#btnEditSubmit", ajaxUpdate);


});