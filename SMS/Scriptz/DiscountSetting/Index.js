

//DISCOUNT SETTING => ADD.JS

$(function () {

    var table;//declared for datatable


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
            { "data": "GroupName" },
            { "data": "CentreCode" },
            { "data": "FromDate" }

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
                "targets": 4,
                "bSortable": false,
                "render": function (data, type, row) {
                    return '<div id="test" >' +
                                '<div class=pull-left">' +
                                    '<a class="btn btn-xs btn-info editData pull-left"  data-group-name=' + row.GroupName + ' >' +
                                    '<i class="fa fa-pencil"></i> Edit</a>' +
                                '</div>' +
                                '<div></div>' +
                                '<div class="">' +
                                    '<a class="btn btn-xs btn-danger deleteData pull-left" data-group-name=' + row.GroupName + ' data-name="' + row.Name + '" >' +
                                    '<i class="fa fa-close"></i> Delete</a>' +
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

    //toastr options
    toastr.options = {
        "closeButton": true,
        "positionClass": "toast-bottom-right",
        "progressBar": true
    }


    var deleteData = function (groupName) {

        var form = $("#frmIndex");
        var href = $(form).data('delete-url');
        $.blockUI({ message: '<h3><img src="../plugins/jQueryBlockUI/images/busy.gif" /> <b>Please wait... </b></h3>' });
        $.ajax({
            type: "POST",
            url: href,
            data: { groupName: groupName },
            datatype: "json",
            success: function (data) {
                $.unblockUI();

                if (data == "success") {
                    toastr.success("Successfully deleted the details.")
                    setTimeout(function () {
                        var url = $(form).data("url");
                        window.location.href = url;
                    }, 2000);
                }
                else {
                    toastr.error("Error:Something gone wrong")
                }
            },
            error: function (data) {
                $.unblockUI();
                toastr.error("Exception:some thing gone wrong");
            }
        });
    }

    var redirectToEdit = function (groupName) {

        var url = $("#frmIndex").data("edit-url");
        url = url.replace('param_placeholder', groupName);
        window.location.href = url;


    };

    var redirectToAdd = function () {

        var btn = $("#btnAddNew");
        var groupcount_href = $(btn).data('groupcount-href');
        var add_href = $(btn).attr('href');
        var group_href = $(btn).data('group-href');

        $.blockUI({ message: '<h3><img src="../plugins/jQueryBlockUI/images/busy.gif" /> <b>Please wait... </b></h3>' });
        $.ajax({
            type: "POST",
            url: groupcount_href,
            datatype: "json",
            success: function (data) {
                $.unblockUI();

                if (data.message > 0) {
                    var url = add_href;
                    window.location.href = url;
                }
                else if (data.message === 0) {
                    bootbox.confirm("Either group is not added or all groups are allotted.Do you wish to create new group", function (result) {
                        if (result) {
                            var url = group_href;
                            window.location.href = url;
                        }

                    });
                }
                else {
                    toastr.error("Error:some thing gone wrong");
                }
            },
            error: function (data) {
                $.unblockUI();
                toastr.error("Exception:some thing gone wrong");
            }
        });

        return false;
    }



    $(document).on("click", ".deleteData", function () {
        var groupName = $(this).data('group-name');
        bootbox.confirm("Are you sure you want to delete discount settings for group - <b>" + groupName + "</b>", function (result) {
            if (result) {
                bootbox.hideAll();
                deleteData(groupName);
            };

        });
    });

    $(document).on("click", ".editData", function () {
        var groupName = $(this).data('group-name');
        redirectToEdit(groupName);
    });

    $(document).on("click", "#btnAddNew", function () {
        redirectToAdd();
        return false;
    });


})