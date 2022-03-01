<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Merlin.ascx.cs" Inherits="Plugins.com_simpledonation.SimpleDonationMerlin" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
			<asp:Literal runat="server" ID="merlinWarning"></asp:Literal>
			<asp:Literal runat="server" id="merlinAutoLaunch"></asp:Literal>
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
    
				{% sql return:'baseAmountRow' %}
					WITH transaction_sums(recent_txns)
					AS (
						SELECT TOP 5 sum(td.Amount) AS recent_txns
						FROM FinancialTransactionDetail td
						INNER JOIN FinancialTransaction t
							ON t.Id = td.TransactionId
						WHERE t.AuthorizedPersonAliasId = {{ CurrentPerson.PrimaryAliasId }} 
						AND t.TransactionTypeValueId = 53
						GROUP BY td.TransactionId
						ORDER BY td.TransactionId DESC
					)
					SELECT CEILING(AVG(recent_txns)) as BaseAmount FROM transaction_sums
				{% endsql %}
			{% endif %}
		</Rock:Lava>
		
		<script type="text/javascript">
			var personRow = {% if CurrentPerson %}{{ sdPersonRow | First | ToJSON }}{% else %}  {
		  "SimpleDonationPersonId": "notLoggedIntoRock"
		}{% endif %};
			var personId = personRow ? personRow['SimpleDonationPersonId'] : 'rockLoggedInNoSdPerson'

			var baseAmountRow = {% if CurrentPerson %}{{ baseAmountRow | First | ToJSON }}{% else %}null{% endif %};
			var baseAmount = baseAmountRow ? baseAmountRow['BaseAmount'] : null;

			window.simpleDonationMerlinSettings = {
				personId: personId,
				baseAmount: baseAmount
			};
			console.log('merlin initialized');
			console.log(window.simpleDonationMerlinSettings);
		</script>

    </ContentTemplate>
</asp:UpdatePanel>