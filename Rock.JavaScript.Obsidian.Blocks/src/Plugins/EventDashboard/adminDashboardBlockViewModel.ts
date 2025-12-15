import { ContentChannelItemBag } from "../ViewModels/contentChannelItemBag"
import { DefinedValueBag } from "../ViewModels/definedValueBag"
import { AttributeBag } from "../ViewModels/attributeBag"
import { PersonBag } from "../ViewModels/personBag"

export type AdminDashboardBlockViewModel = {
    events: ContentChannelItemBag[];
    submittedEvents: ContentChannelItemBag[];
    changedEvents: ContentChannelItemBag[];
    inprogressEvents: ContentChannelItemBag[];
    comments: ContentChannelItemBag[];
    isEventAdmin: boolean;
    isRoomAdmin: boolean;
    locations: DefinedValueBag[];
    ministries: DefinedValueBag[];
    budgetLines: DefinedValueBag[];
    drinks: DefinedValueBag[];
    requestStatus: AttributeBag;
    requestType: AttributeBag;
    workflowURL: string;
    defaultStatuses: string[];
    users: PersonBag[];
};