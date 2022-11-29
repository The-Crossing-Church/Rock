import { DefinedValue, ContentChannelItem, ContentChannelItemAssociation, Attribute } from "../../../ViewModels"

export type AdminDashboardBlockViewModel = {
    events: ContentChannelItem[];
    submittedEvents: ContentChannelItem[];
    changedEvents: ContentChannelItem[];
    inprogressEvents: ContentChannelItem[];
    comments: ContentChannelItem[];
    isEventAdmin: boolean;
    isRoomAdmin: boolean;
    locations: DefinedValue[];
    ministries: DefinedValue[];
    budgetLines: DefinedValue[];
    drinks: DefinedValue[];
    requestStatus: Attribute;
    requestType: Attribute;
    workflowURL: string;
    defaultStatuses: string[];
};