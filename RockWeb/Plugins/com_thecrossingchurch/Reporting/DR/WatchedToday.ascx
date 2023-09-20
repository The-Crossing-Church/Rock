<%@ Control Language="C#" AutoEventWireup="true" CodeFile="WatchedToday.ascx.cs" Inherits="RockWeb.Plugins.com_thecrossingchurch.Reporting.DR.WatchedToday" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbMessage" runat="server" Visible="false" />
        <div class="card mt-4 px-4 pb-4">
            <div class="row">
                <div class="col col-xs-12">
                    <h3>What's On Today</h3>
                    <div class="row" data-masonry='{"percentPosition": true }' id="watched-today-container">

                    </div>
                </div>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>

<script>
    $(document).ready(() => {
        getTodaysInteractions()
    })
    function getTodaysInteractions() {
        $.ajax({
            type: "POST",
            url: window.location + "&isWatchedTodayRequest=true",
            data: "",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (res) {
                let container = document.getElementById("watched-today-container")
                if (res.length == 0) {
                    let div = document.createElement("div")
                    div.classList.add("font-italic")
                    div.classList.add("col-xs-12")
                    div.classList.add("col-md-3")
                    div.innerText = "No content has been viewed today."
                    container.appendChild(div)
                }
                for (let i = 0; i < res.length; i++) {
                    let col = document.createElement("div")
                    col.classList.add("col-xs-12")
                    col.classList.add("col-md-3")
                    let a = document.createElement("a")
                    let well = document.createElement("div")
                    well.classList.add("well")
                    let series = document.createElement("b")
                    series.innerText = res[i][0].Series
                    well.appendChild(series)
                    for (let k = 0; k < res[i].length; k++) {
                        let item = document.createElement("div");
                        item.innerText = res[i][k].Name + " (" + res[i][k].Views + ")"
                        series.appendChild(item)
                    }
                    a.appendChild(well)
                    col.appendChild(a)
                    container.appendChild(col)
                }
            },
            failure: function (res) { console.error(res) }
        })

    }
</script>
<style>

</style>
