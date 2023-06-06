import { DefinedValue, ContentChannelItem, Attribute } from "../../../ViewModels"

export type CalendarBlockViewModel = {
    events: ContentChannelItem[];
    locations: DefinedValue[];
    ministries: DefinedValue[];
    requestStatus: Attribute;
    requestType: Attribute;
    formUrl: String;
    dashboardUrl: String;
};