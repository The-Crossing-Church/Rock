import { DefinedValue, ContentChannelItem, ContentChannelItemAssociation, Attribute } from "../../../ViewModels"

export type AdminDashboardBlockViewModel = {
    events: ContentChannelItem[];
    eventDetails: ContentChannelItemAssociation[];
    isEventAdmin: boolean;
    isRoomAdmin: boolean;
    locations: DefinedValue[];
    ministries: DefinedValue[];
    budgetLines: DefinedValue[];
    drinks: DefinedValue[];
    requestStatus: Attribute;
    requestType: Attribute;
};