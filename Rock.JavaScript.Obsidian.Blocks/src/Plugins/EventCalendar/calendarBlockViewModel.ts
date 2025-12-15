import { ContentChannelItemBag } from "../ViewModels/contentChannelItemBag"
import { DefinedValueBag } from "../ViewModels/definedValueBag"
import { AttributeBag } from "../ViewModels/attributeBag"

export type CalendarBlockViewModel = {
    events: ContentChannelItemBag[];
    locations: DefinedValueBag[];
    ministries: DefinedValueBag[];
    requestStatus: AttributeBag;
    requestType: AttributeBag;
    formUrl: String;
    dashboardUrl: String;
    isEventAdmin: boolean;
    currentPersonId: Number;
};