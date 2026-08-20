import { ContentChannelItemBag } from "../../ViewModels/contentChannelItemBag"
import { DefinedValueBag } from "../../ViewModels/definedValueBag"
import { AttributeBag } from "../../ViewModels/attributeBag"
import { PersonBag } from "../../ViewModels/personBag"

export type DatabaseProviderBlockViewModel = {
    CCId: number;
    events: ContentChannelItemBag[];
    locations: DefinedValueBag[];
    ministries: DefinedValueBag[];
    budgetLines: DefinedValueBag[];
    drinks: DefinedValueBag[];
    inventory: DefinedValueBag[];
    requestStatus: AttributeBag;
    requestType: AttributeBag;
};