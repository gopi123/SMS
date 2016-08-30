
//ProjectReportApproval => LeaderApprovalList
$(function () {
    var table;//declared for datatable

    //toastr options
    toastr.options = {
        "closeButton": true,
        "positionClass": "toast-bottom-right",
        "progressBar": true
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
            { "data": "VerifiedDate" },
            { "data": "InstructorName" },
            { "data": "StudentID" },
            { "data": "StudentName" },
            { "data": "CourseName" },
            { "data": "Id" }

        ],

        //Defining checkbox in columns
        "aoColumnDefs": [
            {
                "targets": [0],
                "bSortable": false
            },
            {
                "targets": 7,
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
                window.location.href = url;
            });

        }
    });

});