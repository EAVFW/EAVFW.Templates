import gen from "manifest/generated";
import { ManifestDefinition, ManifestWithExpressions } from "@eavfw/manifest"
 
export default (gen as ManifestWithExpressions) as any as ManifestDefinition
