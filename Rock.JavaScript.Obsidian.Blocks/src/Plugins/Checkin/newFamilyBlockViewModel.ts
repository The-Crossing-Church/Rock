import { DefinedValueBag } from "@Obsidian/ViewModels/Entities/definedValueBag"
import { DefinedTypeBag } from "@Obsidian/ViewModels/Entities/definedtypeBag"
import { AttributeBag } from "@Obsidian/ViewModels/Entities/attributeBag"
import { PersonBag } from "@Obsidian/ViewModels/Entities/personBag"
import { PhoneNumberBag } from "@Obsidian/ViewModels/Entities/phoneNumberBag"
import { GroupBag } from "@Obsidian/ViewModels/Entities/groupBag"
export type NewFamilyBlockViewModel = {
  showTitle: string;
  titleDefinedType: DefinedTypeBag;
  showNickName: string;
  showMiddleName: string;
  showSuffix: string;
  suffixDefinedType: DefinedTypeBag;
  connectionStatusDefinedType: DefinedTypeBag;
  defaultConnectionStatus: DefinedValueBag;
  requireConnectionStatus: String;
  requireGender: string;
  requireBirthDate: string;
  requireGradeOrAbility: string;
  showMaritalStatus: string;
  maritalStatusDefinedType: DefinedTypeBag;
  defaultAdultMaritalStatus: DefinedValueBag;
  defaultChildMaritalStatus: DefinedValueBag;
  showEmail: boolean;
  showEmailOptOut: boolean;
  showCell: boolean;
  showSMSEnabled: boolean;
  phoneType: DefinedValueBag;
  showAddress: boolean;
  existingPersonPhoneCantBeMessaged: boolean;
  adultAttributes: AttributeBag[];
  childAttributes: AttributeBag[];
  abilityLevelDefinedType: DefinedTypeBag;
  abilityLevelAttribute: AttributeBag;
  gradeDefinedType: DefinedTypeBag;
  graduationYear: number;
  existingPerson: PersonBag;
  emptyPerson: PersonBag;
  existingPersonPhoneNumber: PhoneNumberBag;
  emptyPersonPhoneNumber: PhoneNumberBag;
  Groups: GroupBag[];
  GroupStartDOBAttribute: AttributeBag;
  GroupEndDOBAttribute: AttributeBag;
  GroupAbilityAttribute: AttributeBag;
  GroupGradeAttribute: AttributeBag;
}