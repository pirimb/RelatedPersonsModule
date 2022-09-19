var table = $('#mainDataTable').DataTable({
    initComplete: function () {
        this.api()
            .columns()
            .every(function () {
                var column = this;
                var select = $('<select style="width:100%"><option value=""></option></select>')
                    .appendTo($(column.footer()).empty())
                    .on('change', function () {
                        var val = $.fn.dataTable.util.escapeRegex($(this).val());
                        column.search(val ? '^' + val + '$' : '', true, false).draw();
                    });

                column
                    .data()
                    .unique()
                    .sort()
                    .each(function (d, j) {
                        select.append('<option value="' + d + '">' + d + '</option>');
                    });
            });
    },
    order: [],
    "language": {
        "decimal": "",
        "emptyTable": "Məlumat yoxdur!",
        "info": "_TOTAL_ elementdən _START_ - _END_ arası",
        "infoEmpty": "Boş",
        "infoFiltered": "(filtered from _MAX_ total entries)",
        "infoPostFix": "",
        "thousands": ",",
        "lengthMenu": " _MENU_ element göstər",
        "loadingRecords": "Loading...",
        "processing": "Processing...",
        "search": "Axtar:",
        "zeroRecords": "Axtarış üzrə məlumat tapılmadı",
        "paginate": {
            "first": "First",
            "last": "Last",
            "next": "Sonrakı",
            "previous": "Öncəki"
        },
        "aria": {
            "sortAscending": ": activate to sort column ascending",
            "sortDescending": ": activate to sort column descending"
        }
    },
    "scrollX": false, "ordering": true,
    searching: true, paging: true, info: true,
});

$(document).ready(function () {
    $.ajax({
        url: '/Home/GetUserNameFromSession',
        type: 'GET',
        dataType: 'text',
        success: function (data) {
            $('#username').text(data);
        },
    });
});

$(document).ready(function () {
    $.ajax({
        url: '/Home/GetDbName',
        type: 'GET',
        dataType: 'text',
        success: function (data) {
            $('#database').text(data);
        },
    });
});


var url = window.location;

$('ul.nav-sidebar a').filter(function () {
    return this.href == url;
}).addClass('active');

$(document).ready(function () {
    $('.js-example-basic-multiple').select2();
});


$("#addNewRelatedPersonBtn").click(function () {
    var str = $("#pin_code").val();
    if (str.length != 0) {
        $.ajax({
            url: "/Home/CheckByPinCode",
            type: "GET",
            data: { pinCode: str },
            dataType: 'text',
            success: function (result) {
                if (result == 1) {
                    alert("Qeyd olunan pin kod Aidiyyatı şəxslər bölməsində mövcuddur!");
                    window.location.href = "/Home/Index";
                }
            }
        });
    }
});