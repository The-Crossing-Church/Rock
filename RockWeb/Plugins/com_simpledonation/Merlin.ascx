<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Merlin.ascx.cs" Inherits="Plugins.com_simpledonation.SimpleDonationMerlin" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
		<asp:Literal runat="server" ID="merlinWarning"></asp:Literal>

        <asp:Panel ID="pnlView" runat="server"> 
            <div class="panel-body">
                    <asp:Literal runat="server" id="merlinControl"></asp:Literal>
            </div>
        </asp:Panel>

		<Rock:Lava runat="server" ID="currentPersonCheck">
			{% if CurrentPerson %}
				{% sql return:'sdPersonRow' %}
					SELECT TOP 1 sdp.SimpleDonationPersonId
					FROM [FinancialPersonSavedAccount] fpsa
					JOIN _com_simpledonation_SimpleDonationPerson sdp
						ON sdp.PaymentMethodId = fpsa.ReferenceNumber
					WHERE fpsa.PersonAliasId = {{ CurrentPerson.PrimaryAliasId }}
					ORDER BY fpsa.CreatedDateTime DESC
				{% endsql %}
			{% endif %}
		
			<script type="text/javascript">
				var personRow = {% if CurrentPerson %}{ { sdPersonRow | First | ToJSON } } {% else %} {
					"SimpleDonationPersonId": "notLoggedIntoRock"
				} {% endif %};
				var personId = personRow ? personRow['SimpleDonationPersonId'] : 'rockLoggedInNoSdPerson'

				window.simpleDonationMerlinSettings = {
					personId: personId
				};
				console.log('merlin initialized');
				console.log(window.simpleDonationMerlinSettings);
			</script>
		</Rock:Lava>

		<asp:Literal runat="server" id="merlinAutoLaunch"></asp:Literal>
    </ContentTemplate>
</asp:UpdatePanel>