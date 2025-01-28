import { ContentChannelItemBag } from "@Obsidian/ViewModels/Entities/contentChannelItemBag"
import { DefinedValueBag } from "@Obsidian/ViewModels/Entities/definedValueBag"
import { AttributeBag } from "@Obsidian/ViewModels/Entities/attributeBag"

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