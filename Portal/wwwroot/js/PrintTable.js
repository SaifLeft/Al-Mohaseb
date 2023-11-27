let PrintTableBtn = $('.PrintTableBtn')

if (PrintTableBtn) {
    PrintTableBtn.on('click', function () {
        // Get the DataTable instance
        var table = $('.PrintDataTable');

        if (!table || !PrintTableTitle) {
            return;
        }
        var table = table.DataTable();

        // Export data with column header formatting
        var data = table.buttons.exportData({
            format: {
                header: function (data, columnIdx) {
                    return data;
                }
            }
        });

        // Open a new window for printing
        var printWin = window.open('', '', 'width=1000,height=600');

        // Print the data on the page after a short delay
        printWin.onload = function () {
            setTimeout(function () {
                printWin.document.write('<html st dir="rtl" style="font-family:Adobe Arabic;">');

                printWin.document.write('<title>' + PrintTableTitle + '</title>');

                printWin.document.write('<h1>' + PrintTableTitle + '</h1>');
                printWin.document.write('<table border="1" dir="rtl" class="table table-striped table-bordered" style="width:100%">');
                printWin.document.write('<thead>');
                printWin.document.write('<tr>');
                // Use the column headers from DataTable
                $.each(data.header, function (i, header) {
                    printWin.document.write('<th>' + header + '</th>');
                });
                printWin.document.write('</tr>');
                printWin.document.write('</thead>');
                printWin.document.write('<tbody>');
                // Iterate over the data body and print each row
                $.each(data.body, function (i, row) {
                    printWin.document.write('<tr>');
                    $.each(row, function (j, cell) {
                        printWin.document.write('<td>' + cell + '</td>');
                    });
                    printWin.document.write('</tr>');
                });

                printWin.document.write('</tbody>');
                printWin.document.write('</table>');
                // عدد الأسماء في النظام
                printWin.document.write('<h3>عدد الأسماء : ' + data.body.length + '</h3>');



                printWin.document.close();

                // Focus on the print window and initiate the print
                printWin.focus();
                printWin.print();
                printWin.close();
            }, 500); // Adjust the delay time as needed
        };
    });

}