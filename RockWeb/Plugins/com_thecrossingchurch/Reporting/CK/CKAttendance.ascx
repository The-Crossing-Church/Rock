<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CKAttendance.ascx.cs" Inherits="RockWeb.Plugins.com_thecrossingchurch.Reporting.CK.CKAttendance" %>
<script>
    function notes() {
        $('.add-note').click(function (e) {
            e.preventDefault();
            window.open($(this).attr('href'), 'fbShareWindow', 'height=450, width=550, top=' + ($(window).height() / 2 - 275) + ', left=' + ($(window).width() / 2 - 225) + ', toolbar=0, location=0, menubar=0, directories=0, scrollbars=0');
            return false;
        });
    }
</script>
<script src="https://cdnjs.cloudflare.com/ajax/libs/exceljs/4.3.0/exceljs.min.js" integrity="sha512-UnrKxsCMN9hFk7M56t4I4ckB4N/2HHi0w/7+B/1JsXIX3DmyBcsGpT3/BsuZMZf+6mAr0vP81syWtfynHJ69JA==" crossorigin="anonymous" referrerpolicy="no-referrer"></script>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">Attendance Filter</h1>
            </div>
            <div class="panel-body">
                <div class="row">
                    <div class="col-md-3">
                        <Rock:DatePicker ID="stDate" runat="server" Label="Start Date" Required="true" />
                    </div>
                    <div class="col-md-3">
                        <Rock:DatePicker ID="endDate" runat="server" Label="End Date" Required="true" />
                    </div>
                    <div class="col-md-6">
                        <asp:Panel ID="pnlSchedules" runat="server" CssClass="form-group">
                            <label class="control-label">Service Times(s)</label>
                            &nbsp;
                            <asp:LinkButton ID="lbRefreshSchedules" runat="server" OnClick="sdrpDates_SelectedDateRangeChanged" CausesValidation="false"><i class="fa fa-sm fa-sync"></i></asp:LinkButton>
                            <div class="control-wrapper">
                                <asp:Literal ID="lSchedules" runat="server" />
                                <Rock:RockListBox ID="lbSchedules" runat="server" SelectionMode="Multiple" Visible="true" RequiredErrorMessage="Service Time(s) is Required"
                                    DataValueField="Id" DataTextField="Name" CssClass="input-width-md" />
                            </div>
                        </asp:Panel>
                    </div>
                </div>
                <div class="row">
                    <div class="pull-right">
                        <Rock:BootstrapButton ID="btnFilter" runat="server" Text="Filter" CssClass="btn btn-primary" OnClientClick="notes();" OnClick="btnFilter_Click" />
                    </div>
                </div>
            </div>
        </div>
        <br />
        <div class="custom-container" id="DataContainer" runat="server" visible="false">
            <asp:PlaceHolder ID="phContent" runat="server" Visible="false" />
        </div>
        <div class="row mt-3 ">
            <div class="pull-right">
                <button class="btn btn-primary" id="btnExport" runat="server" visible="false" onclick="exportToXLS();return false;">Export to Excel</button>
                <%--<Rock:BootstrapButton Visible="false" ID="btnExport" runat="server" Text="Export" CssClass="btn btn-primary d-none" OnClick="btnExport_Click" />--%>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
<div class="modal" id="att-modal" tabindex="-1" role="dialog">
    <div class="modal-dialog" role="document">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title" id="att-modal-title"></h5>
                <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                    <span aria-hidden="true">&times;</span>
                </button>
            </div>
            <div class="modal-body" id="att-modal-body"></div>
        </div>
    </div>
</div>
<script>
    function displayGroups(el) {
        $('#att-modal #att-modal-title').text($(el)[0].parentElement.parentElement.firstChild.innerText)
        $('#att-modal #att-modal-body').html(el.dataset.content + "<div class='row'><div class='col col-xs-8'><b>Total</b></div><div class='col col-xs-4'><b>" + el.innerText + "</b></div></div>")
        $('#att-modal').modal()
    }
    function exportToXLS() {
        let elements = document.querySelectorAll('.att-schedule > div')
        let workbook = new ExcelJS.Workbook()
        let sheet = workbook.addWorksheet('Attendance')
        elements.forEach((el) => {
            let data = []
            for (let child of el.children) {
                data.push(child.innerText + (child.classList.contains("att-closed") ? "**" : "") + (child.classList.contains("att-threshold") ? "__" : ""))
            }
            sheet.addRow(data)
        })
        sheet._rows.forEach((row) => {
            row._cells.forEach((cell) => {
                if (cell._value.value.includes('**')) {
                    cell._value.value = cell._value.value.replace('**', '')
                    cell.fill = {
                        type: 'pattern',
                        pattern: 'solid',
                        fgColor: { argb: 'ffd3afaf' },
                    }
                } else if (cell._value.value.includes('__')) {
                    cell._value.value = cell._value.value.replace('__', '')
                    cell.fill = {
                        type: 'pattern',
                        pattern: 'solid',
                        fgColor: { argb: 'ffd3d3d3' },
                    }
                }
                let reg = /^[0-9]+$/
                if (cell._value.value.match(reg)) {
                    sheet.getCell(cell._address).value = parseInt(cell._value.value)
                }
            })
        })
        sheet.columns[0].width = 30
        sheet.views = [{ state: 'frozen', xSplit: 2, ySplit: 0 }]
        workbook.xlsx.writeBuffer().then(function (data) {
            let blob = new Blob([data], { type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" })
            let link = document.createElement("a")
            link.href = window.URL.createObjectURL(blob)
            link.download = "CK Attendance Report"
            link.style = "display: none;"
            document.body.appendChild(link)
            link.click()
        })
    }
</script>
<style>
    .custom-container {
        background-color: #fff;
        padding: 8px;
        border-radius: 8px;
        box-shadow: 0 0 1px 0 rgb(0 0 0 / 8%), 0 1px 3px 0 rgb(0 0 0 / 15%);
    }

    .att-container {
        overflow-x: scroll;
        overflow-y: hidden;
    }

    .att-schedule {
        width: max-content;
    }

    .att-schedule-title {
        font-weight: bold;
        font-size: 18px;
    }

    .att-location, .att-schedule-header, .att-schedule-total {
        display: flex;
    }

    .att-location {
        border-bottom: 1px solid grey;
    }

    .att-threshold, .att-total-threshold {
        background-color: lightgrey;
        font-weight: bold;
    }

    .att-date-threshold, .att-threshold, .att-total-threshold {
        position: sticky;
        left: 225px;
    }

    .att-schedule-title, .att-location-title {
        min-width: 225px;
    }

    .att-location-title {
        height: 30px;
        display: flex;
        align-items: center;
    }

    .att-location-title, .att-schedule-title {
        position: sticky;
        left: 0;
        z-index: 10;
        background-color: white;
    }

    .att-data, .att-data div, .att-threshold, .att-date, .att-date-threshold, .att-total-data, .att-total-threshold {
        min-width: 75px;
    }

        .att-data div, .att-threshold, .att-date, .att-date-threshold, .att-total-threshold, .att-total-data {
            height: 30px;
            display: flex;
            align-items: center;
            justify-content: center;
        }

    .att-data {
        cursor: pointer;
    }

    .att-closed {
        background-color: #d3afaf;
    }

    .att-date-threshold {
        background-color: #fff;
    }

    .no-display {
        display: none;
    }
</style>
