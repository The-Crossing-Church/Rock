import { DefinedValueBag } from "../ViewModels/definedValueBag"
import { ContentChannelItemBag } from "../ViewModels/contentChannelItemBag"
import { ContentChannelItemAssociationBag } from "../ViewModels/contentChannelItemAssociationBag"
import { AttributeMatrixBag } from "../ViewModels/attributeMatrixBag"
import { AttributeMatrixItemBag } from "../ViewModels/attributeMatrixItemBag"
import { AttributeBag } from "../ViewModels/attributeBag"

export type SubmissionFormBlockViewModel = {
    request: ContentChannelItemBag;
    originalRequest: ContentChannelItemBag;
    events: ContentChannelItemBag[];
    existing: ContentChannelItemBag[];
    existingDetails: ContentChannelItemAssociationBag[];
    isSuperUser: boolean;
    isEventAdmin: boolean;
    isRoomAdmin: boolean;
    permissions: string[];
    locations: DefinedValueBag[];
    locationSetupMatrix: AttributeMatrixBag[];
    locationSetupMatrixItem: AttributeMatrixItemBag[];
    ministries: DefinedValueBag[];
    budgetLines: DefinedValueBag[];
    inventoryList: DefinedValueBag[];
    adminDashboardURL: string;
    userDashboardURL: string;
    discountCodeAttrs: AttributeBag[];
};