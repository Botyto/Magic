require "PageNode"

Class.DocsMapNode = {
	__inherit = "PageNode",

	elements = false,
}

function DocsMapNode:__ctor()
	self.elements = { }
end

function DocsMapNode:GetElementsLists()
	local by_letter = { }

	for i,elem in ipairs(self.elements) do
		local first_char = string.upper(string.sub(elem.title, 1, 1))
		by_letter[first_char] = by_letter[first_char] or { }
		table.insert(by_letter[first_char], elem)
	end

	local function SortComp(a, b)
		return a < b
	end

	for k,list in pairs(by_letter) do
		table.sort(list, SortComp)
	end

	return by_letter
end

function DocsMapNode:GeneratePageContent(writer)
	local page = { }

	if self.summary and self.summary ~= "" then
		table.insert(page, self.summary)
	end

	for letter,list in pairs(self:GetElementsLists()) do
		local links_list = { }
		for i,elem in ipairs(list) do
			if IsKindOf(elem, "PageNode") then
				table.insert(links_list, self:GenerateLinkTo(writer, elem))
			end
		end

		table.insert(page, writer:GenerateSection(letter))
		table.insert(page, writer:GenerateList(links_list))
	end

	return page
end
